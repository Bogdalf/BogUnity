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
    [SerializeField] private float strengthToAttackRatio = 1f; // 1 strength = 1 attack damage
    [SerializeField] private float vitalityToHealthRatio = 10f; // 1 vitality = 10 max health

    void Start()
    {
        CalculateStats();
        InitializeHealth();
    }

    void CalculateStats()
    {
        // Calculate total stats
        float totalStrength = baseStrength + bonusStrength;
        float totalVitality = baseVitality + bonusVitality;

        // Convert to derived stats
        attackDamage = totalStrength * strengthToAttackRatio;
        maxHealth = totalVitality * vitalityToHealthRatio;

        Debug.Log("Stats Calculated - Strength: " + totalStrength + " | Attack Damage: " + attackDamage);
        Debug.Log("Vitality: " + totalVitality + " | Max Health: " + maxHealth);
    }

    void InitializeHealth()
    {
        // Set player's max health based on vitality
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.SetMaxHealth(maxHealth);
        }
    }

    // Getters
    public float GetAttackDamage()
    {
        return attackDamage;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public float GetStrength()
    {
        return baseStrength + bonusStrength;
    }

    public float GetVitality()
    {
        return baseVitality + bonusVitality;
    }

    public void SetEquipmentBonuses(float equipStrength, float equipVitality)
    {
        bonusStrength = equipStrength;
        bonusVitality = equipVitality;
        CalculateStats();
        InitializeHealth();
    }

    // Modifiers (for talents/gear later)
    public void AddStrength(float amount)
    {
        bonusStrength += amount;
        CalculateStats();
        Debug.Log("Added " + amount + " Strength. New total: " + GetStrength());
    }

    public void AddVitality(float amount)
    {
        bonusVitality += amount;
        CalculateStats();
        InitializeHealth();
        Debug.Log("Added " + amount + " Vitality. New total: " + GetVitality());
    }
}