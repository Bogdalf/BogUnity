using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamageable, IStunnable
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float health = 3f;
    [SerializeField] private float damage = 1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private bool startAggressive = false; // If true, enemy is always aggressive

    private Transform player;
    private Rigidbody2D rb;
    private bool isAggressive = false;

    // Stun system
    private bool isStunned = false;
    private float stunTimeRemaining = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // If set to start aggressive, activate immediately
        isAggressive = startAggressive;
    }

    void Update()
    {
        // Update stun timer
        if (isStunned)
        {
            stunTimeRemaining -= Time.deltaTime;
            if (stunTimeRemaining <= 0f)
            {
                EndStun();
            }
            return; // Don't process other logic while stunned
        }

        // Check if player is in range (if not already aggressive)
        if (!isAggressive && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                BecomeAggressive();
            }
        }
    }

    void FixedUpdate()
    {
        // Don't move if stunned
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Only move toward player when aggressive
        if (isAggressive && player != null)
        {
            // Move toward player
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Stay still when not aggressive
            rb.linearVelocity = Vector2.zero;
        }
    }

    void BecomeAggressive()
    {
        isAggressive = true;
        Debug.Log(gameObject.name + " detected player and is now aggressive!");
    }

    public void Stun(float duration)
    {
        isStunned = true;
        stunTimeRemaining = duration;

        // Visual feedback - change color to yellow
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.yellow;
        }

        Debug.Log(gameObject.name + " is stunned for " + duration + " seconds!");
    }

    void EndStun()
    {
        isStunned = false;
        stunTimeRemaining = 0f;

        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        Debug.Log(gameObject.name + " stun ended!");
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        // If hit, immediately become aggressive
        if (!isAggressive)
        {
            BecomeAggressive();
        }

        // Spawn damage number
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damageAmount, false);
        }

        // Visual feedback - flash red (unless stunned)
        if (!isStunned)
        {
            StartCoroutine(FlashRed());
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = originalColor;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only damage player when aggressive and not stunned
        if (isAggressive && !isStunned && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}