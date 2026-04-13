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

    // Temporary multipliers applied by buffs (1.0 = no buff)
    private float strengthMultiplier     = 1f;
    private float intelligenceMultiplier = 1f;
    private float focusMultiplier        = 1f;

    // Derived values
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
        attackDamage = (GetStrength() + GetIntelligence() + GetFocus()) * attackPerStatPoint;
        maxHealth    = GetVitality() * vitalityToHealthRatio;
    }

    void InitializeHealth()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
            health.SetMaxHealth(maxHealth, fullyHeal: !initialized);
    }

    // ─── Gear Bonuses ─────────────────────────────────────────────────────────────

    public void SetRuneBookBonuses(float str, float intel, float focus, float vit)
    {
        bonusStrength     = str;
        bonusIntelligence = intel;
        bonusFocus        = focus;
        bonusVitality     = vit;
        CalculateStats();
        InitializeHealth();
    }

    // ─── Talent Bonuses ───────────────────────────────────────────────────────────

    public void AddStrength(float amount)     { bonusStrength     += amount; CalculateStats(); }
    public void AddIntelligence(float amount) { bonusIntelligence += amount; CalculateStats(); }
    public void AddFocus(float amount)        { bonusFocus        += amount; CalculateStats(); }
    public void AddVitality(float amount)     { bonusVitality     += amount; CalculateStats(); InitializeHealth(); }

    // ─── Buff Multipliers ─────────────────────────────────────────────────────────

    /// <summary>
    /// Sets a temporary multiplier on Strength (e.g. 1.2 = +20%).
    /// Pass 1.0 to remove the buff. Recalculates stats immediately.
    /// </summary>
    public void SetStrengthMultiplier(float multiplier)
    {
        strengthMultiplier = Mathf.Max(0f, multiplier);
        CalculateStats();
    }

    public void SetIntelligenceMultiplier(float multiplier)
    {
        intelligenceMultiplier = Mathf.Max(0f, multiplier);
        CalculateStats();
    }

    public void SetFocusMultiplier(float multiplier)
    {
        focusMultiplier = Mathf.Max(0f, multiplier);
        CalculateStats();
    }

    // ─── Stat Totals ──────────────────────────────────────────────────────────────

    /// <summary>Total Strength including gear, talents, and active buffs.</summary>
    public float GetStrength()     => (baseStrength     + bonusStrength)     * strengthMultiplier;
    public float GetIntelligence() => (baseIntelligence + bonusIntelligence) * intelligenceMultiplier;
    public float GetFocus()        => (baseFocus        + bonusFocus)        * focusMultiplier;
    public float GetVitality()     => baseVitality      + bonusVitality;

    // ─── Derived Stats ────────────────────────────────────────────────────────────

    public float GetAttackDamage() => attackDamage;
    public float GetMaxHealth()    => maxHealth;

    // ─── Aspect Power ─────────────────────────────────────────────────────────────

    public float GetWarPower()           => GetStrength();
    public float GetSorceryPower()       => GetIntelligence();
    public float GetAmplificationPower() => GetFocus();
}