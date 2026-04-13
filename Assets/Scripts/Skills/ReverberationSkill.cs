using System.Collections;
using UnityEngine;

/// <summary>
/// Reverberation — War Aspect starter ability.
/// A slam that deals AoE damage in a circle around the player.
/// Damage fires on animation event at frame 20.
/// Movement is locked for the duration of the animation.
/// On hit, triggers ReverberationBuff which grants +20% Strength for 10 seconds.
/// </summary>
public class ReverberationSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data")]
    [SerializeField] private SkillData skillData;

    [Header("AoE Settings")]
    [SerializeField] private float aoeRadius = 2.5f;
    [SerializeField] private float flatDamageBonus = 0f;

    [Header("Hit Feel")]
    [SerializeField] private float hitStopDuration  = 0.12f;
    [SerializeField] private float hitStopTimeScale = 0.05f;

    private PlayerStats playerStats;
    private PlayerTalents playerTalents;
    private PlayerMovement playerMovement;
    private Rigidbody2D rb;
    private ReverberationBuff buff;
    private Animator animator;

    private float lastUseTime = -999f;
    private bool isActive = false;
    private bool hitRegistered = false;
    public bool IsActive() => isActive;

    public SkillData SkillData => skillData;

    void Start()
    {
        playerStats    = GetComponent<PlayerStats>();
        playerTalents  = GetComponent<PlayerTalents>();
        playerMovement = GetComponent<PlayerMovement>();
        rb             = GetComponent<Rigidbody2D>();
        animator       = GetComponent<Animator>();

        // Ensure buff component exists
        buff = GetComponent<ReverberationBuff>();
        if (buff == null)
            buff = gameObject.AddComponent<ReverberationBuff>();

        // Register with ActionBar so it can be assigned to slots
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

        // Lock movement for the slam duration
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Kill existing momentum so the player doesn't slide during the animation
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Reverberation");

        Debug.Log("Reverberation activated!");
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

        float damage = CalculateDamage();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius);
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

        Debug.Log($"Reverberation hit {enemiesHit} enemies for {damage} damage.");

        if (enemiesHit > 0)
            StartCoroutine(HitStop(hitStopDuration, hitStopTimeScale));

        // Activate Strength buff regardless of hit count
        buff?.Activate();
    }

    /// <summary>
    /// Called by Animation Event on the last frame.
    /// Restores movement and clears active state.
    /// </summary>
    public void OnReverberationEnd()
    {
        isActive = false;

        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    // ─── Damage ───────────────────────────────────────────────────────────────────

    float CalculateDamage()
    {
        float warPower = playerStats != null ? playerStats.GetWarPower() : 0f;

        // Apply Aspect damage bonus from talents
        float aspectBonus = 0f;
        if (playerTalents != null)
            aspectBonus = playerTalents.GetAspectDamageBonus(AspectType.War);

        float damage = warPower;
        if (aspectBonus > 0f)
            damage *= 1f + (aspectBonus / 100f);

        return damage;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.2f, 0.2f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }

    IEnumerator HitStop(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
    }
}