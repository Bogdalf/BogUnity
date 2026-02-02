using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    private float maxHealth = 100f; // Remove [SerializeField], we'll set this from stats
    private float currentHealth;

    [Header("Damage Settings")]
    [SerializeField] private float invulnerabilityTime = 1f; // Invincible after taking damage
    private float lastDamageTime = -999f;

    private PlayerDash playerDash;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerDash = GetComponent<PlayerDash>(); // Add this line
    }

    void Update()
    {
        // Flash red when invulnerable
        if (Time.time < lastDamageTime + invulnerabilityTime)
        {
            float alpha = Mathf.PingPong(Time.time * 10f, 1f);
            spriteRenderer.color = new Color(1f, alpha, alpha);
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    public void TakeDamage(float damage)
    {
        // Check if dashing and invulnerable
        if (playerDash != null && playerDash.IsDashing())
        {
            return;
        }

        // Check invulnerability timer
        if (Time.time < lastDamageTime + invulnerabilityTime)
        {
            return;
        }

        currentHealth -= damage;
        lastDamageTime = Time.time;

        // Spawn damage number
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damage, true);
        }

        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("PLAYER DIED!");
        // Reload the scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        currentHealth = maxHealth; // Heal to full when max health changes
        Debug.Log("Max health set to: " + maxHealth);
    }
}