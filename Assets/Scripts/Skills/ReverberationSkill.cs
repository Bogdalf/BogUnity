using System.Collections;
using UnityEngine;

/// <summary>
/// Reverberation — War Aspect starter ability.
/// A slam that deals AoE damage in a circle around the player.
/// Damage fires on animation event at frame 20.
/// Movement is locked for the duration of the animation.
/// On hit, triggers ReverberationBuff which grants +20% Strength for 10 seconds.
/// Radius can be increased by talent nodes (ReverberationRadiusBonus effect type).
///
/// Keystone — Echo Pulse:
/// Each enemy hit by the initial slam adds one echo pulse (capped at 3).
/// Each pulse fires one second apart, dealing full damage with no movement lock.
/// </summary>
public class ReverberationSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data")]
    [SerializeField] private SkillData skillData;

    [Header("AoE Settings")]
    [SerializeField] private float aoeRadius       = 2.5f;
    [SerializeField] private float flatDamageBonus = 0f;

    [Header("VFX")]
    [SerializeField] private Animator AttackTelegraphs;

    [Header("Hit Feel")]
    [SerializeField] private float hitStopDuration  = 0.12f;
    [SerializeField] private float hitStopTimeScale = 0.05f;

    private PlayerStats    playerStats;
    private PlayerTalents  playerTalents;
    private PlayerMovement playerMovement;
    private Rigidbody2D    rb;
    private ReverberationBuff buff;
    private Animator       animator;

    private Vector3 baseTelegraphScale;
    private Vector3 cachedAttackDirection;

    private float lastUseTime   = -999f;
    private bool  isActive      = false;
    private bool  hitRegistered = false;

    private const int MaxEchoPulses = 3;

    public SkillData SkillData => skillData;
    public bool IsActive()     => isActive;

    void Start()
    {
        playerStats    = GetComponent<PlayerStats>();
        playerTalents  = GetComponent<PlayerTalents>();
        playerMovement = GetComponent<PlayerMovement>();
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();

        if (AttackTelegraphs != null)
            baseTelegraphScale = AttackTelegraphs.transform.localScale;

        buff = GetComponent<ReverberationBuff>();
        if (buff == null)
            buff = gameObject.AddComponent<ReverberationBuff>();

        ActionBar.Instance?.RegisterSkill(this);
    }

    // ─── ISkill ───────────────────────────────────────────────────────────────────

    public bool CanExecute()
    {
        if (isActive) return false;
        if (PersistentInputManager.Instance != null &&
            PersistentInputManager.Instance.IsCombatInputBlocked()) return false;
        return Time.time >= lastUseTime + skillData.cooldown;
    }

    public void Execute()
    {
        if (!CanExecute()) return;

        isActive      = true;
        hitRegistered = false;
        lastUseTime   = Time.time;

        if (playerMovement != null) playerMovement.enabled = false;
        if (rb != null)             rb.linearVelocity      = Vector2.zero;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cachedAttackDirection = (mousePos - transform.position).normalized;

        if (AttackTelegraphs != null)
        {
            float scaleMultiplier = GetCurrentRadius() / aoeRadius;
            AttackTelegraphs.transform.localScale = baseTelegraphScale * scaleMultiplier;
            AttackTelegraphs.SetTrigger("Reverb");
            AttackTelegraphs.SetFloat("DirectionX", cachedAttackDirection.x);
            AttackTelegraphs.SetFloat("DirectionY", cachedAttackDirection.y);
        }

        if (animator != null)
        {
            animator.SetTrigger("Reverberation");
            animator.SetFloat("DirectionX", cachedAttackDirection.x);
            animator.SetFloat("DirectionY", cachedAttackDirection.y);
        }
    }

    public float GetCooldownPercent()
    {
        if (isActive) return 1f;
        float timeSince = Time.time - lastUseTime;
        if (timeSince >= skillData.cooldown) return 0f;
        return 1f - (timeSince / skillData.cooldown);
    }

    // ─── Animation Events ─────────────────────────────────────────────────────────

    /// <summary>
    /// Called by Animation Event at frame 20 — when the slam visually lands.
    /// </summary>
    public void OnReverberationHit()
    {
        if (hitRegistered) return;
        hitRegistered = true;

        float damage        = CalculateDamage();
        float currentRadius = GetCurrentRadius();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentRadius);
        int enemiesHit = 0;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                enemiesHit++;
            }
        }

        Debug.Log($"Reverberation hit {enemiesHit} enemies for {damage} damage. Radius: {currentRadius}");

        if (enemiesHit > 0)
            StartCoroutine(HitStop(hitStopDuration, hitStopTimeScale));

        // Only the initial hit activates the buff
        buff?.Activate();

        // Queue echo pulses if keystone is learned
        if (playerTalents != null && playerTalents.HasReverberationEchoPulse() && enemiesHit > 0)
        {
            int pulseCount = Mathf.Clamp(enemiesHit, 0, MaxEchoPulses);
            StartCoroutine(EchoPulseSequence(pulseCount, damage, currentRadius));
        }
    }

    /// <summary>
    /// Called by Animation Event on the last frame.
    /// </summary>
    public void OnReverberationEnd()
    {
        isActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (AttackTelegraphs != null)
            AttackTelegraphs.transform.localScale = baseTelegraphScale;
    }

    // ─── Echo Pulses ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Fires pulseCount echo pulses, one per second.
    /// No movement lock — player moves freely during pulses.
    /// </summary>
    IEnumerator EchoPulseSequence(int pulseCount, float damage, float radius)
    {
        for (int i = 0; i < pulseCount; i++)
        {
            yield return new WaitForSeconds(.5f);
            FireEchoPulse(damage, radius);
        }
    }

    void FireEchoPulse(float damage, float radius)
    {
        // Find nearest enemy for pulse direction
        Vector2 pulseDirection = Vector2.down;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, radius * 2f);
        float closestDist = float.MaxValue;

        foreach (Collider2D nearby in nearbyEnemies)
        {
            if (!nearby.CompareTag("Enemy")) continue;
            float dist = Vector2.Distance(transform.position, nearby.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                pulseDirection = (nearby.transform.position - transform.position).normalized;
            }
        }

        if (AttackTelegraphs != null)
        {
            GameObject vfxClone = Instantiate(
                AttackTelegraphs.gameObject,
                AttackTelegraphs.transform.position,
                AttackTelegraphs.transform.rotation,
                AttackTelegraphs.transform.parent
            );
            float scaleMultiplier = radius / aoeRadius;
            Vector3 worldScale = AttackTelegraphs.transform.lossyScale * scaleMultiplier;
            // Detach so the VFX stays in world space while player moves
            vfxClone.transform.SetParent(null);
            vfxClone.transform.localScale = worldScale;

            Animator cloneAnimator = vfxClone.GetComponent<Animator>();
            StartCoroutine(FireCloneAnimation(cloneAnimator, pulseDirection, vfxClone));
        }

        // Damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        int enemiesHit = 0;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                enemiesHit++;
            }
        }

        if (enemiesHit > 0)
            StartCoroutine(HitStop(hitStopDuration * 0.5f, hitStopTimeScale));

        Debug.Log($"Reverberation echo pulse hit {enemiesHit} enemies for {damage} damage.");
    }
    IEnumerator FireCloneAnimation(Animator cloneAnimator, Vector2 direction, GameObject clone)
    {
        yield return null; // wait one frame for the animator to initialize

        if (cloneAnimator != null)
        {
            cloneAnimator.SetTrigger("Reverb");
            cloneAnimator.SetFloat("DirectionX", direction.x);
            cloneAnimator.SetFloat("DirectionY", direction.y);
        }

        Destroy(clone, 1.5f);
    }
    // ─── Radius / Damage ──────────────────────────────────────────────────────────

    float GetCurrentRadius()
    {
        float bonus = playerTalents != null ? playerTalents.GetReverberationRadiusBonus() : 0f;
        return aoeRadius + bonus;
    }

    float CalculateDamage()
    {
        float warPower    = playerStats != null ? playerStats.GetWarPower() : 0f;
        float aspectBonus = playerTalents != null
            ? playerTalents.GetAspectDamageBonus(AspectType.War)
            : 0f;

        float damage = warPower + flatDamageBonus;
        if (aspectBonus > 0f)
            damage *= 1f + (aspectBonus / 100f);

        return damage;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);

        float currentRadius = GetCurrentRadius();
        if (currentRadius > aoeRadius)
        {
            Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, currentRadius);
        }
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────────

    IEnumerator HitStop(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}