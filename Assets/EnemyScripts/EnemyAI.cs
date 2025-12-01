using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamageable
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // If set to start aggressive, activate immediately
        isAggressive = startAggressive;
    }

    void Update()
    {
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

        // Visual feedback - flash red
        StartCoroutine(FlashRed());

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
        // Only damage player when aggressive
        if (isAggressive && collision.gameObject.CompareTag("Player"))
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