using System.Collections;
using UnityEngine;

/// <summary>
/// Base class for all bosses. Handles health, phase transitions, damage, death,
/// and the entrance trigger called by scene sequence scripts.
///
/// Derive from this and override the virtual methods for boss-specific behavior.
/// The specific boss AI script (e.g. KhatunBossAI) handles attack patterns and movement.
/// </summary>
public class BossBase : MonoBehaviour, IDamageable
{
    [Header("Identity")]
    [SerializeField] private string bossName = "???";

    [Header("Health")]
    [SerializeField] protected float maxHealth = 500f;
    protected float currentHealth;

    [Header("Phases")]
    [Tooltip("Health % thresholds that trigger phase changes. E.g. 0.66 and 0.33 = 3 phases.")]
    [SerializeField] protected float[] phaseThresholds = { 0.66f, 0.33f };

    [Header("Hit Feel")]
    [SerializeField] private float hitStopDuration  = 0.05f;
    [SerializeField] private float hitStopTimeScale = 0.1f;
    [SerializeField] private float flashDuration    = 0.1f;

    [Header("Death")]
    [SerializeField] private string deathStoryFlag = "";
    [SerializeField] private float deathSequenceDelay = 1.5f;

    [Header("References")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected SpriteRenderer spriteRenderer;

    public int CurrentPhase   { get; protected set; } = 1;
    public bool IsDead        { get; protected set; } = false;
    public bool IsActive      { get; protected set; } = false;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        if (animator == null)       animator       = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // ─── Entrance ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by the scene sequence script when the intro finishes.
    /// Override to start AI, play entrance animation, etc.
    /// </summary>
    public virtual void TriggerEntrance()
    {
        IsActive = true;

        // Register with the boss health bar UI
        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.RegisterBoss(this, bossName);

        Debug.Log($"{gameObject.name} entrance triggered.");
    }

    // ─── IDamageable ──────────────────────────────────────────────────────────────

    public virtual void TakeDamage(float amount)
    {
        if (IsDead || !IsActive) return;

        currentHealth -= amount;
        currentHealth  = Mathf.Max(0f, currentHealth);

        if (DamageNumberManager.Instance != null)
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, amount, false);

        StartCoroutine(FlashHit());
        StartCoroutine(HitStop(hitStopDuration, hitStopTimeScale));

        if (animator != null)
            animator.SetTrigger("Hurt");

        CheckPhaseTransition();

        if (currentHealth <= 0f)
            StartCoroutine(DieSequence());
    }

    // ─── Phases ───────────────────────────────────────────────────────────────────

    void CheckPhaseTransition()
    {
        if (phaseThresholds == null || phaseThresholds.Length == 0) return;

        float healthPercent = currentHealth / maxHealth;
        int newPhase = 1;

        for (int i = 0; i < phaseThresholds.Length; i++)
        {
            if (healthPercent <= phaseThresholds[i])
                newPhase = i + 2; // Phase 1 is default, thresholds start phase 2, 3, etc.
        }

        if (newPhase != CurrentPhase)
        {
            CurrentPhase = newPhase;
            OnPhaseChange(newPhase);
        }
    }

    /// <summary>
    /// Called when the boss transitions to a new phase.
    /// Override to change attack patterns, speed, add new abilities, etc.
    /// </summary>
    protected virtual void OnPhaseChange(int newPhase)
    {
        Debug.Log($"{gameObject.name} entered phase {newPhase}!");
    }

    // ─── Death ────────────────────────────────────────────────────────────────────

    IEnumerator DieSequence()
    {
        IsDead  = true;
        IsActive = false;

        // Disable physics
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Die");

        OnDeath();

        yield return new WaitForSeconds(deathSequenceDelay);

        // Set story flag
        if (!string.IsNullOrEmpty(deathStoryFlag) && GameStateManager.Instance != null)
            GameStateManager.Instance.SetQuestFlag(deathStoryFlag, true);

        AfterDeathSequence();
    }

    /// <summary>
    /// Called immediately on death — override for death VFX, sounds, etc.
    /// </summary>
    protected virtual void OnDeath()
    {
        Debug.Log($"{gameObject.name} has died.");

        if (BossHealthBarUI.Instance != null)
            BossHealthBarUI.Instance.UnregisterBoss();
    }

    /// <summary>
    /// Called after the death sequence delay — override to destroy the boss,
    /// trigger a cutscene, spawn loot, etc.
    /// </summary>
    protected virtual void AfterDeathSequence()
    {
        Destroy(gameObject);
    }

    // ─── Getters / Setters ────────────────────────────────────────────────────────

    public float GetHealthPercent() => currentHealth / maxHealth;
    public float GetCurrentHealth() => currentHealth;
    public float GetMaxHealth()     => maxHealth;

    /// <summary>
    /// Sets the boss's max health — called by BossIntermissionManager
    /// to apply the statue health bonus before Boss2 activates.
    /// </summary>
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth     = newMaxHealth;
        currentHealth = newMaxHealth;
        Debug.Log($"{gameObject.name} max health set to {maxHealth}");
    }

    // ─── Coroutines ───────────────────────────────────────────────────────────────

    IEnumerator FlashHit()
    {
        if (spriteRenderer != null) spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(flashDuration);
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    IEnumerator HitStop(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    protected virtual void OnDrawGizmosSelected() { }
}