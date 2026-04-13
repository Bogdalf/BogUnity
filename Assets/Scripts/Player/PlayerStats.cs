using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseStrength = 10f;
    [SerializeField] private float baseIntelligence = 10f;
    [SerializeField] private float baseFocus = 10f;
    [SerializeField] private float baseVitality = 10f;

    [Header("Stat Scaling")]
    [SerializeField] private float attackPerStatPoint = 1f;
    [SerializeField] private float vitalityToHealthRatio = 10f;

    // Bonus stats from gear and talents
    private float bonusStrength = 0f;
    private float bonusIntelligence = 0f;
    private float bonusFocus = 0f;
    private float bonusVitality = 0f;

    // Derived values (recalculated whenever stats change)
    private float attackDamage;
    private float maxHealth;

    private bool initialized = false;

    void Start()
    {
        CalculateStats();
        InitializeHealth();
        initialized = true;
    }

    void CalculateStats()
    {
        // All three offensive stats contribute equally to attack power
        attackDamage = (GetStrength() + GetIntelligence() + GetFocus()) * attackPerStatPoint;
        maxHealth = GetVitality() * vitalityToHealthRatio;
    }

    void InitializeHealth()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SetMaxHealth(maxHealth, fullyHeal: !initialized);
        }
    }

    // ─── Gear Bonuses ───────────────────────────────────────────────────────────

    /// <summary>
    /// Called by PlayerRuneBook when a book is equipped or unequipped.
    /// </summary>
    public void SetRuneBookBonuses(float str, float intel, float focus, float vit)
    {
        bonusStrength = str;
        bonusIntelligence = intel;
        bonusFocus = focus;
        bonusVitality = vit;
        CalculateStats();
        InitializeHealth();
    }

    // ─── Talent Bonuses ──────────────────────────────────────────────────────────

    public void AddStrength(float amount)
    {
        bonusStrength += amount;
        CalculateStats();
    }

    public void AddIntelligence(float amount)
    {
        bonusIntelligence += amount;
        CalculateStats();
    }

    public void AddFocus(float amount)
    {
        bonusFocus += amount;
        CalculateStats();
    }

    public void AddVitality(float amount)
    {
        bonusVitality += amount;
        CalculateStats();
        InitializeHealth();
    }

    // ─── Stat Totals ─────────────────────────────────────────────────────────────

    public float GetStrength()     => baseStrength     + bonusStrength;
    public float GetIntelligence() => baseIntelligence + bonusIntelligence;
    public float GetFocus()        => baseFocus        + bonusFocus;
    public float GetVitality()     => baseVitality     + bonusVitality;

    // ─── Derived Stats ────────────────────────────────────────────────────────────

    /// <summary>
    /// Total attack power used by basic melee attacks.
    /// Combines all three offensive stats equally.
    /// </summary>
    public float GetAttackDamage() => attackDamage;

    public float GetMaxHealth() => maxHealth;

    // ─── Aspect Power ─────────────────────────────────────────────────────────────
    // Aspect abilities use these instead of GetAttackDamage(), so a War ability
    // scales only from Strength, a Sorcery ability from Intelligence, etc.

    /// <summary>War Aspect abilities scale from Strength.</summary>
    public float GetWarPower()           => GetStrength();

    /// <summary>Sorcery Aspect abilities scale from Intelligence.</summary>
    public float GetSorceryPower()       => GetIntelligence();

    /// <summary>Amplification Aspect abilities scale from Focus.</summary>
    public float GetAmplificationPower() => GetFocus();
}