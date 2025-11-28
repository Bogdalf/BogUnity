using UnityEngine;

public class PlayerEquipment : MonoBehaviour
{
    [Header("Current Equipment")]
    [SerializeField] private WeaponData mainHandWeapon;
    [SerializeField] private WeaponData offHandWeapon; // Can be weapon or shield

    private PlayerStats playerStats;
    private PlayerMelee playerMelee;

    // Cached values
    private float totalMinDamage;
    private float totalMaxDamage;
    private float attackSpeed;
    private float equipmentBonusStrength; // Add this
    private float equipmentBonusVitality; // Add this

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerMelee = GetComponent<PlayerMelee>();

        CalculateEquipmentStats();
        ApplyEquipmentStats();
    }

    void CalculateEquipmentStats()
    {
        totalMinDamage = 0f;
        totalMaxDamage = 0f;
        attackSpeed = 0.5f; // Default

        float totalBonusStrength = 0f;
        float totalBonusVitality = 0f;

        // Main hand weapon
        if (mainHandWeapon != null)
        {
            totalMinDamage += mainHandWeapon.minDamage;
            totalMaxDamage += mainHandWeapon.maxDamage;
            attackSpeed = mainHandWeapon.attackCooldown;

            // Apply weapon mastery speed bonus
            attackSpeed = ApplyWeaponMasterySpeed(attackSpeed, mainHandWeapon.weaponClass);

            totalBonusStrength += mainHandWeapon.bonusStrength;
            totalBonusVitality += mainHandWeapon.bonusVitality;

            Debug.Log("Main Hand: " + mainHandWeapon.weaponName + " (Base Speed: " + mainHandWeapon.attackCooldown + ", Modified: " + attackSpeed + ")");
        }

        // Off hand (weapon or shield)
        if (offHandWeapon != null)
        {
            if (offHandWeapon.weaponType == WeaponType.OneHanded)
            {
                totalMinDamage += offHandWeapon.minDamage;
                totalMaxDamage += offHandWeapon.maxDamage;

                // Average the attack speeds with mastery applied
                if (mainHandWeapon != null)
                {
                    float offHandSpeed = ApplyWeaponMasterySpeed(offHandWeapon.attackCooldown, offHandWeapon.weaponClass);
                    attackSpeed = (attackSpeed + offHandSpeed) / 2f;
                    Debug.Log("Off Hand Weapon: " + offHandWeapon.weaponName + " (Speed: " + offHandSpeed + ")");
                    Debug.Log("Dual Wield - Averaged Attack Speed: " + attackSpeed);
                }
            }
            else if (offHandWeapon.weaponType == WeaponType.Shield)
            {
                Debug.Log("Off Hand Shield: " + offHandWeapon.weaponName);
            }

            totalBonusStrength += offHandWeapon.bonusStrength;
            totalBonusVitality += offHandWeapon.bonusVitality;
        }

        Debug.Log("Total Weapon Damage: " + totalMinDamage + "-" + totalMaxDamage);
        Debug.Log("Attack Speed: " + attackSpeed);
        Debug.Log("Equipment Bonuses - Strength: " + totalBonusStrength + " | Vitality: " + totalBonusVitality);

        // Store bonuses to apply
        equipmentBonusStrength = totalBonusStrength;
        equipmentBonusVitality = totalBonusVitality;
    }

    void ApplyEquipmentStats()
    {
        // Apply stat bonuses to PlayerStats
        if (playerStats != null)
        {
            playerStats.SetEquipmentBonuses(equipmentBonusStrength, equipmentBonusVitality);
        }

        // Apply attack speed to PlayerMelee
        if (playerMelee != null)
        {
            playerMelee.SetAttackSpeed(attackSpeed);
        }
    }

    // Getters for damage calculation
    public float GetWeaponDamage()
    {
        return Random.Range(totalMinDamage, totalMaxDamage);
    }

    public bool HasWeaponEquipped()
    {
        return mainHandWeapon != null;
    }

    // Equipment management
    public void EquipMainHand(WeaponData weapon)
    {
        // If equipping 2H, remove offhand
        if (weapon != null && weapon.weaponType == WeaponType.TwoHanded)
        {
            offHandWeapon = null;
        }

        mainHandWeapon = weapon;
        CalculateEquipmentStats();
        ApplyEquipmentStats();
    }

    public void EquipOffHand(WeaponData weapon)
    {
        // Can't use offhand with 2H weapon
        if (mainHandWeapon != null && mainHandWeapon.weaponType == WeaponType.TwoHanded)
        {
            Debug.Log("Can't equip offhand with two-handed weapon!");
            return;
        }

        offHandWeapon = weapon;
        CalculateEquipmentStats();
        ApplyEquipmentStats();
    }
    public bool IsDualWielding()
    {
        return mainHandWeapon != null &&
               offHandWeapon != null &&
               offHandWeapon.weaponType == WeaponType.OneHanded;
    }

    public float GetMainHandDamage()
    {
        if (mainHandWeapon != null)
        {
            return mainHandWeapon.GetRandomDamage();
        }
        return 0f;
    }

    float ApplyWeaponMasterySpeed(float baseSpeed, WeaponClass weaponClass)
    {
        PlayerTalents talents = GetComponent<PlayerTalents>();
        if (talents != null && weaponClass != WeaponClass.None)
        {
            float speedBonus = talents.GetWeaponMasterySpeedBonus(weaponClass);
            if (speedBonus > 0)
            {
                // Speed bonus reduces cooldown (5% faster = 0.95x cooldown)
                float modifiedSpeed = baseSpeed * (1f - (speedBonus / 100f));
                Debug.Log("Weapon Speed Mastery: -" + speedBonus + "% cooldown (" + baseSpeed + " -> " + modifiedSpeed + ")");
                return modifiedSpeed;
            }
        }
        return baseSpeed;
    }

    public float GetOffHandDamage()
    {
        if (offHandWeapon != null && offHandWeapon.weaponType == WeaponType.OneHanded)
        {
            return offHandWeapon.GetRandomDamage();
        }
        return 0f;
    }

    public WeaponClass GetMainHandWeaponClass()
    {
        if (mainHandWeapon != null)
        {
            return mainHandWeapon.weaponClass;
        }
        return WeaponClass.None;
    }

    public WeaponClass GetOffHandWeaponClass()
    {
        if (offHandWeapon != null)
        {
            return offHandWeapon.weaponClass;
        }
        return WeaponClass.None;
    }
}