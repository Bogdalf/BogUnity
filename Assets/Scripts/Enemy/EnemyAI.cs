using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamageable, IStunnable
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed    = 2f;
    [SerializeField] private float health       = 3f;
    [SerializeField] private float stunDuration = 0.5f;

    [Header("Detection")]
    [SerializeField] private float detectionRange  = 5f;
    [SerializeField] private bool  startAggressive = false;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce    = 5f;
    [SerializeField] private float knockbackDuration = 0.1f;
    [Header("Attack Hitboxes")]
    [SerializeField] private Collider2D attackHitbox_N;
    [SerializeField] private Collider2D attackHitbox_S;
    [SerializeField] private Collider2D attackHitbox_E;
    [SerializeField] private Collider2D attackHitbox_W;

    [Header("Slam Hitboxes")]
    [SerializeField] private Collider2D slamHitbox_N;
    [SerializeField] private Collider2D slamHitbox_S;
    [SerializeField] private Collider2D slamHitbox_E;
    [SerializeField] private Collider2D slamHitbox_W;

    private Collider2D activeHitbox;
    [Header("Attack")]
    [SerializeField] private float attackRange   = 1.2f;
    [SerializeField] private float attackDamage  = 1f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Slam Attack")]
    [SerializeField] private float slamRange    = 2f;
    [SerializeField] private float slamDamage   = 3f;
    [SerializeField] private float slamCooldown = 4f;
    [Header("Slam Telegraph")]
    [SerializeField] private GameObject slamTelegraph;
    [SerializeField] private float telegraphDuration = 0.5f; // Should match your first slam clip length

    private float lastAttackTime = -999f;
    private float lastSlamTime   = -999f;
    private bool  isAttacking    = false;

    private Transform     player;
    private Rigidbody2D   rb;
    private Animator      animator;
    private bool          isAggressive = false;

    // Stun
    private bool          isStunned        = false;
    private float         stunTimeRemaining = 0f;
    private SpriteRenderer spriteRenderer;
    public bool IsStunned() => isStunned;
    Collider2D GetDirectionalHitbox(Vector2 direction, bool isSlam)
    {
        // Determine dominant axis
        bool horizontal = Mathf.Abs(direction.x) > Mathf.Abs(direction.y);

        if (isSlam)
        {
            if (horizontal)
                return direction.x > 0 ? slamHitbox_E : slamHitbox_W;
            else
                return direction.y > 0 ? slamHitbox_N : slamHitbox_S;
        }
        else
        {
            if (horizontal)
                return direction.x > 0 ? attackHitbox_E : attackHitbox_W;
            else
                return direction.y > 0 ? attackHitbox_N : attackHitbox_S;
        }
    }
    
    void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator       = GetComponent<Animator>();
        player         = GameObject.FindGameObjectWithTag("Player")?.transform;
        isAggressive   = startAggressive;
        if (slamTelegraph != null)
            slamTelegraph.SetActive(false);
    }

    void Update()
    {
        if (CombatPauseManager.IsPaused) return;

        if (isStunned)
        {
            stunTimeRemaining -= Time.deltaTime;
            if (stunTimeRemaining <= 0f)
                EndStun();
            return;
        }

        if (!isAggressive && player != null)
        {
            if (Vector2.Distance(transform.position, player.position) <= detectionRange)
                BecomeAggressive();
        }

        if (isAggressive && player != null && !isAttacking)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            // Attack priority: Slam first if in range and off cooldown, then basic attack
            if (CanSlam() && dist <= slamRange)
            {
                StartSlam();
            }
            else if (CanAttack() && dist <= attackRange)
            {
                StartAttack();
            }
        }

        // Update animator
        if (isAggressive && player != null)
        {
            float speed = isAttacking ? 0f : rb.linearVelocity.magnitude;
            if (animator != null)
            {
                animator.SetFloat("Speed", speed);

                if (speed > 0.1f)
                {
                    Vector2 direction = (player.position - transform.position).normalized;
                    animator.SetFloat("MovementX", direction.x);
                    animator.SetFloat("MovementY", direction.y);
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (CombatPauseManager.IsPaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isStunned || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isAggressive && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            // Chase until within basic attack range
            if (dist > attackRange)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ─── Attack ───────────────────────────────────────────────────────────────────

    bool CanAttack() => !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    bool CanSlam()   => !isAttacking && Time.time >= lastSlamTime   + slamCooldown;

    void StartAttack()
    {
        isAttacking    = true;
        lastAttackTime = Time.time;

        if (player != null && animator != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            animator.SetFloat("MovementX", direction.x);
            animator.SetFloat("MovementY", direction.y);

            EnableHitbox(GetDirectionalHitbox(direction, false));
        }

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    void StartSlam()
    {
        isAttacking  = true;
        lastSlamTime = Time.time;

        if (player != null && animator != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            animator.SetFloat("MovementX", direction.x);
            animator.SetFloat("MovementY", direction.y);

            EnableHitbox(GetDirectionalHitbox(direction, false));
        }

        if (animator != null)
            animator.SetTrigger("Charge");
        if (slamTelegraph != null)
        {
            slamTelegraph.SetActive(true);
            // Scale to match slam range
            //slamTelegraph.transform.localScale = Vector3.one * (slamRange * 2f);
        }
    }
    // HITBOXES
    void EnableHitbox(Collider2D hitbox)
    {
        if (activeHitbox != null)
            activeHitbox.gameObject.SetActive(false);

        activeHitbox = hitbox;

        if (activeHitbox != null)
            activeHitbox.gameObject.SetActive(true);
    }

    void DisableActiveHitbox()
    {
        if (activeHitbox != null)
        {
            activeHitbox.gameObject.SetActive(false);
            activeHitbox = null;
        }
    }
    // ─── Animation Events ─────────────────────────────────────────────────────────

    /// <summary>Called by Animation Event at the hit frame of the Attack animation.</summary>
    public void OnAttackHit()
    {
        if (activeHitbox == null || player == null) return;

        // Check if player is inside the active hitbox
        if (activeHitbox.bounds.Contains(player.position))
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            playerHealth?.TakeDamage(attackDamage);
        }

        DisableActiveHitbox();
    }

    /// <summary>Called by Animation Event at the hit frame of the Slam animation.</summary>
    public void OnSlamHit()
    {
        if (slamTelegraph != null)
            slamTelegraph.SetActive(false);

        if (activeHitbox != null && player != null)
        {
            if (activeHitbox.bounds.Contains(player.position))
            {
                PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
                playerHealth?.TakeDamage(slamDamage);
            }
        }

        DisableActiveHitbox();
    }

    /// <summary>Called by Animation Event on the last frame of any attack animation.</summary>
    public void OnAttackEnd()
    {
        DisableActiveHitbox();
        isAttacking = false;
    }

    // ─── Aggression ───────────────────────────────────────────────────────────────

    void BecomeAggressive()
    {
        isAggressive = true;
    }

    // ─── IDamageable ──────────────────────────────────────────────────────────────

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, amount, false);

        // Knockback
        if (player != null && rb != null)
        {
            Vector2 knockbackDir = (transform.position - player.position).normalized;
            rb.linearVelocity    = Vector2.zero;
            rb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }

        if (health <= 0f)
            Die();
    }

    void Die()
    {
        if (animator != null)
            animator.SetTrigger("Die");

        Destroy(gameObject, 0.5f);
    }

    // ─── IStunnable ───────────────────────────────────────────────────────────────

    public void Stun(float duration)
    {
        isStunned         = true;
        stunTimeRemaining = duration > 0 ? duration : stunDuration;
        rb.linearVelocity = Vector2.zero;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;
    }

    void EndStun()
    {
        isStunned = false;
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        // Detection range — yellow
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Basic attack range — red
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Slam range — magenta (larger)
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, slamRange);
    }
}