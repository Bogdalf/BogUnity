using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    private float maxHealth = 100f;
    private float currentHealth;

    [Header("Damage Settings")]
    [SerializeField] private float invulnerabilityTime = 1f;
    private float lastDamageTime = -999f;

    private PlayerDash playerDash;
    private PlayerBlink playerBlink;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Don't set currentHealth here — SetMaxHealth is called by PlayerStats
        // and will initialize it correctly on Start
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerDash = GetComponent<PlayerDash>();
        playerBlink = GetComponent<PlayerBlink>();
    }

    void Update()
    {
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
        // Check ability i-frames
        if (playerDash != null && playerDash.IsDashing()) return;
        if (playerBlink != null && playerBlink.IsBlinking()) return;

        // Check invulnerability window
        if (Time.time < lastDamageTime + invulnerabilityTime) return;

        currentHealth -= damage;
        lastDamageTime = Time.time;

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damage, true);

        Debug.Log("Player Health: " + currentHealth + " / " + maxHealth);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("PLAYER DIED!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;

    /// <summary>
    /// Called by PlayerStats when stats are recalculated.
    /// Only sets currentHealth if this is the first initialization.
    /// Subsequent calls (from equipping gear) will NOT heal the player.
    /// </summary>
    public void SetMaxHealth(float newMaxHealth, bool fullyHeal = false)
    {
        float previousMax = maxHealth;
        maxHealth = newMaxHealth;

        if (fullyHeal || currentHealth <= 0)
        {
            // Full heal only on first init or explicit request
            currentHealth = maxHealth;
        }
        else
        {
            // Proportionally scale current health when max changes
            // (e.g. going from 100/100 to 150 gives 150, but 80/100 → 120/150)
            float healthPercent = currentHealth / previousMax;
            currentHealth = Mathf.Round(healthPercent * maxHealth);
        }

        Debug.Log($"Max health set to: {maxHealth}, current: {currentHealth}");
    }

    /// <summary>
    /// Restore health by a fixed amount, capped at max.
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Healed {amount}. Current health: {currentHealth}");
    }
}