using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamageable, IStunnable
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float health = 3f;
    [SerializeField] private float stunDuration = .5f;
    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private bool startAggressive = false;

    

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackDamage = 1f;
    [SerializeField] private float attackCooldown = 1.5f; // Longer than animation to give player a window

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private Transform player;
    private Rigidbody2D rb;
    private Animator animator;
    private bool isAggressive = false;

    // Stun
    private bool isStunned = false;
    private float stunTimeRemaining = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        isAggressive = startAggressive;
    }

    void Update()
    {
        // Stun timer
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

        if (isAggressive && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // In attack range and cooldown ready — start attack
            if (distanceToPlayer <= attackRange && CanAttack())
            {
                StartAttack();
            }

            // Update animator speed and direction
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
        if (isStunned || isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isAggressive && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            // Only chase if outside attack range
            if (distanceToPlayer > attackRange)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = direction * moveSpeed;
            }
            else
            {
                // In attack range — stand still and face player
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    void StartAttack()
{
    isAttacking = true;
    lastAttackTime = Time.time;

    // Cache direction to player at moment of attack
    // so blend tree knows which direction to play
    if (player != null)
    {
        Vector2 direction = (player.position - transform.position).normalized;
        animator.SetFloat("MovementX", direction.x);
        animator.SetFloat("MovementY", direction.y);
    }

    if (animator != null)
        animator.SetTrigger("Attack");
}

    /// <summary>
    /// Called by Animation Event at frame 17 of the attack animation.
    /// Add this as an Animation Event on the enemy's Attack clip.
    /// </summary>
    public void OnAttackHit()
    {
        if (player == null) return;

        // Check player is still in range at the moment of impact
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer > attackRange) return;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }

    /// <summary>
    /// Called by Animation Event at the last frame of the attack animation.
    /// Signals that the swing is fully complete and the enemy can act again.
    /// </summary>
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    void BecomeAggressive()
    {
        isAggressive = true;
        Debug.Log(gameObject.name + " is now aggressive!");
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        if (!isAggressive)
            BecomeAggressive();

        Stun(stunDuration);

        if (animator != null)
            animator.SetTrigger("Hurt");

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damageAmount, false);

        StartCoroutine(FlashRed());

        if (health <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }

    System.Collections.IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    // IStunnable
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimeRemaining = duration;
        isAttacking = false; // Cancel any active attack

        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;
    }

    void EndStun()
    {
        isStunned = false;
        stunTimeRemaining = 0f;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;
    }

    public bool IsStunned() => isStunned;

    void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}