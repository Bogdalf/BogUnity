using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseStrength = 10f;
    [SerializeField] private float baseVitality = 10f;

    [Header("Stat Modifiers")]
    private float bonusStrength = 0f;
    private float bonusVitality = 0f;

    // Calculated values
    private float attackDamage;
    private float maxHealth;

    [Header("Stat Scaling")]
    [SerializeField] private float strengthToAttackRatio = 1f;
    [SerializeField] private float vitalityToHealthRatio = 10f;

    private bool initialized = false;

    void Start()
    {
        CalculateStats();
        InitializeHealth();
        initialized = true;
    }

    void CalculateStats()
    {
        float totalStrength = baseStrength + bonusStrength;
        float totalVitality = baseVitality + bonusVitality;

        attackDamage = totalStrength * strengthToAttackRatio;
        maxHealth = totalVitality * vitalityToHealthRatio;
    }

    void InitializeHealth()
    {
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            // fullyHeal=true only on first init, so equipment changes don't restore health
            health.SetMaxHealth(maxHealth, fullyHeal: !initialized);
        }
    }

    // Getters
    public float GetAttackDamage() => attackDamage;
    public float GetMaxHealth() => maxHealth;
    public float GetStrength() => baseStrength + bonusStrength;
    public float GetVitality() => baseVitality + bonusVitality;

    public void SetEquipmentBonuses(float equipStrength, float equipVitality)
    {
        bonusStrength = equipStrength;
        bonusVitality = equipVitality;
        CalculateStats();
        InitializeHealth(); // Will NOT fully heal since initialized = true
    }

    public void AddStrength(float amount)
    {
        bonusStrength += amount;
        CalculateStats();
    }

    public void AddVitality(float amount)
    {
        bonusVitality += amount;
        CalculateStats();
        InitializeHealth();
    }
}