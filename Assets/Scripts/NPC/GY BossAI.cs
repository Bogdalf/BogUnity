using System.Collections;
using UnityEngine;

/// <summary>
/// Example boss AI demonstrating the full boss pattern.
/// Copy and rename this for each new boss.
///
/// Phase 1 (100-66%): Boss channels a spell facing south, firing meteor bursts.
///                    Boss is fully stationary. Ends when health drops below threshold.
/// Phase 2 (66-33%): Boss stops channeling, begins chasing and attacking.
/// Phase 3 (33-0%):  Faster movement, all attacks available.
/// </summary>
public class GYBossAI : BossBase
{   
    
    [Header("Movement")]
    [SerializeField] private float moveSpeedPhase2 = 2.5f;
    [SerializeField] private float moveSpeedPhase3 = 4f;

    [Header("Melee Attack")]
    [SerializeField] private float meleeRange    = 1.5f;
    [SerializeField] private float meleeDamage   = 15f;
    [SerializeField] private float meleeCooldown = 2f;

    [Header("Phase 3 Attack")]
    [SerializeField] private float attack2Range    = 4f;
    [SerializeField] private float attack2Damage   = 25f;
    [SerializeField] private float attack2Cooldown = 5f;

    [Header("VFX")]
    [SerializeField] private Animator vfxAnimator;

    private Rigidbody2D rb;
    private Transform player;

    private float currentMoveSpeed = 0f;
    private bool isAttacking       = false;
    private bool isChanneling      = false;

    private float lastMeleeTime   = -999f;
    private float lastAttack2Time = -999f;

    private BossMeteorAttack meteorAttack;

    // ─── Lifecycle ────────────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        rb           = GetComponent<Rigidbody2D>();
        meteorAttack = GetComponent<BossMeteorAttack>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        // TEMP — press T to trigger entrance for testing
        if (Input.GetKeyDown(KeyCode.T))
            TriggerEntrance();
        
        if (CombatPauseManager.IsPaused) return;
        if (IsDead || !IsActive)         return;
        if (player == null)              return;

        switch (CurrentPhase)
        {
            case 1: UpdatePhase1(); break;
            case 2: UpdatePhase2(); break;
            case 3: UpdatePhase3(); break;
        }
    }

    void FixedUpdate()
    {
        if (CombatPauseManager.IsPaused || IsDead || !IsActive)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Phase 1 — fully locked
        if (CurrentPhase == 1 || isChanneling || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (player == null) return;

        // Phases 2+ — chase player
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > meleeRange)
        {
            Vector2 direction     = (player.position - transform.position).normalized;
            rb.linearVelocity     = direction * currentMoveSpeed;
            UpdateMovementAnimator(direction);
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ─── Entrance ─────────────────────────────────────────────────────────────────

    public override void TriggerEntrance()
    {
        base.TriggerEntrance();

        if (animator != null)
            animator.SetTrigger("Entrance");

        StartCoroutine(EntranceDelay());

    }

    IEnumerator EntranceDelay()
    {
        IsActive = false;
        yield return new WaitForSeconds(1.5f);
        IsActive = true;
        BeginChannel();
    }

    // ─── Phase 1 — Channel ────────────────────────────────────────────────────────

    void BeginChannel()
    {
        isChanneling = true;

        if (animator != null)
            animator.SetTrigger("StartChannel");

        Debug.Log("Boss began channeling.");
    }

    void EndChannel()
    {
        if (!isChanneling) return;
        isChanneling = false;

        if (animator != null)
            animator.SetTrigger("EndChannel");

        Debug.Log("Boss ended channel.");
    }

    void UpdatePhase1()
    {
        // Boss is stationary and channeling — just fire meteors
        if (!isChanneling) return;

        if (meteorAttack != null && meteorAttack.CanUse())
            meteorAttack.StartMeteorAttack();
    }

    // ─── Phase 2 ──────────────────────────────────────────────────────────────────

    void UpdatePhase2()
    {
        UpdateCombatAnimator();

        if (isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (CanUseMelee(dist))
            StartCoroutine(MeleeRoutine());
    }

    // ─── Phase 3 ──────────────────────────────────────────────────────────────────

    void UpdatePhase3()
    {
        UpdateCombatAnimator();

        if (isAttacking) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Also fires meteors in phase 3
        if (meteorAttack != null && meteorAttack.CanUse())
        {
            meteorAttack.StartMeteorAttack();
            return;
        }

        if (CanUseAttack2(dist))
        {
            StartCoroutine(Attack2Routine());
            return;
        }

        if (CanUseMelee(dist))
            StartCoroutine(MeleeRoutine());
    }

    // ─── Phase Changes ────────────────────────────────────────────────────────────

    protected override void OnPhaseChange(int newPhase)
    {
        base.OnPhaseChange(newPhase);

        switch (newPhase)
        {
            case 2:
                // End the channel — boss wakes up and starts moving
                EndChannel();
                currentMoveSpeed = moveSpeedPhase2;

                if (animator != null)
                    animator.SetTrigger("PhaseTransition");

                Debug.Log("Boss Phase 2 — channel ended, now aggressive!");
                break;

            case 3:
                currentMoveSpeed = moveSpeedPhase3;

                if (animator != null)
                    animator.SetTrigger("PhaseTransition");

                Debug.Log("Boss Phase 3 — enraged!");
                break;
        }
    }

    // ─── Attacks ──────────────────────────────────────────────────────────────────

    bool CanUseMelee(float dist)   => !isAttacking && dist <= meleeRange   && Time.time >= lastMeleeTime   + meleeCooldown;
    bool CanUseAttack2(float dist) => !isAttacking && dist <= attack2Range  && Time.time >= lastAttack2Time + attack2Cooldown;

    IEnumerator MeleeRoutine()
    {
        isAttacking   = true;
        lastMeleeTime = Time.time;

        FacePlayer();

        if (animator != null) animator.SetTrigger("Attack1");

        yield return new WaitUntil(() => !isAttacking);
    }

    IEnumerator Attack2Routine()
    {
        isAttacking     = true;
        lastAttack2Time = Time.time;

        FacePlayer();

        if (animator != null) animator.SetTrigger("Attack2");

        yield return new WaitUntil(() => !isAttacking);
    }

    // ─── Animation Events ─────────────────────────────────────────────────────────

    public void OnMeleeHit()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > meleeRange) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        ph?.TakeDamage(meleeDamage);
    }

    public void OnAttack2Hit()
    {
        if (player == null) return;
        if (Vector2.Distance(transform.position, player.position) > attack2Range) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        ph?.TakeDamage(attack2Damage);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    // ─── Death ────────────────────────────────────────────────────────────────────

    protected override void AfterDeathSequence()
    {
        StartCoroutine(IntermissionDelay());
    }

    IEnumerator IntermissionDelay()
    {
        yield return new WaitForSeconds(3f); // adjust to match your death sequence
        //BossIntermissionManager.Instance?.TriggerIntermission();
        gameObject.SetActive(false); // hide Boss1
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    void FacePlayer()
    {
        if (player == null || animator == null) return;
        Vector2 dir = (player.position - transform.position).normalized;
        animator.SetFloat("DirectionX", dir.x);
        animator.SetFloat("DirectionY", dir.y);
    }

    void UpdateMovementAnimator(Vector2 direction)
    {
        if (animator == null) return;
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
        animator.SetFloat("MovementX", direction.x);
        animator.SetFloat("MovementY", direction.y);
    }

    void UpdateCombatAnimator()
    {
        if (animator == null) return;
        float speed = isAttacking ? 0f : rb.linearVelocity.magnitude;
        animator.SetFloat("Speed", speed);

        if (speed > 0.1f && player != null)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            animator.SetFloat("MovementX", dir.x);
            animator.SetFloat("MovementY", dir.y);
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attack2Range);
    }
}