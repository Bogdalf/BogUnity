using UnityEngine;

public class EnemyAI : MonoBehaviour, IDamageable, IStunnable
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float health = 3f;
    [SerializeField] private float damage = 1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private bool startAggressive = false;

    [Header("Contact Damage")]
    [SerializeField] private float contactDamageCooldown = 1f; // Seconds between contact hits
    private float lastContactDamageTime = -999f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isAggressive = false;

    private bool isStunned = false;
    private float stunTimeRemaining = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        isAggressive = startAggressive;
    }

    void Update()
    {
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
    }

    void FixedUpdate()
    {
        if (isStunned)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isAggressive && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
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

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damageAmount, false);

        if (!isStunned)
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

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!isAggressive || isStunned) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        // Rate-limited contact damage
        if (Time.time < lastContactDamageTime + contactDamageCooldown) return;

        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            lastContactDamageTime = Time.time;
        }
    }

    // IStunnable
    public void Stun(float duration)
    {
        isStunned = true;
        stunTimeRemaining = duration;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.yellow;

        Debug.Log(gameObject.name + " stunned for " + duration + "s");
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}