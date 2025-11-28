using UnityEngine;
using System.Collections.Generic;

public class PlayerTalents : MonoBehaviour
{
    [Header("Talent Points")]
    [SerializeField] private int availableTalentPoints = 10; // Start with some for testing

    [Header("Learned Talents")]
    private Dictionary<TalentData, int> learnedTalents = new Dictionary<TalentData, int>();

    private PlayerStats playerStats;
    private PlayerEquipment playerEquipment;
    private PlayerMelee playerMelee;
    private PlayerDash playerDash;
    private PlayerCharge playerCharge;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerEquipment = GetComponent<PlayerEquipment>();
        playerMelee = GetComponent<PlayerMelee>();
        playerDash = GetComponent<PlayerDash>();
        playerCharge = GetComponent<PlayerCharge>();
    }

    public bool CanLearnTalent(TalentData talent)
    {
        if (talent == null) return false;
        if (availableTalentPoints <= 0) return false;

        // Check if already at max rank
        int currentRank = GetTalentRank(talent);
        if (currentRank >= talent.maxRank) return false;

        // Check prerequisite
        if (talent.prerequisiteTalent != null)
        {
            if (GetTalentRank(talent.prerequisiteTalent) <= 0)
            {
                Debug.Log("Missing prerequisite: " + talent.prerequisiteTalent.talentName);
                return false;
            }
        }

        return true;
    }

    public void LearnTalent(TalentData talent)
    {
        if (!CanLearnTalent(talent)) return;

        // Add or increment rank
        if (learnedTalents.ContainsKey(talent))
        {
            learnedTalents[talent]++;
        }
        else
        {
            learnedTalents[talent] = 1;
        }

        availableTalentPoints--;

        ApplyTalentEffect(talent);

        Debug.Log("Learned: " + talent.talentName + " (Rank " + learnedTalents[talent] + ")");
    }

    void ApplyTalentEffect(TalentData talent)
    {
        switch (talent.effectType)
        {
            case TalentEffectType.IncreaseStrength:
                if (playerStats != null)
                {
                    // We'll need to add a method to add permanent stat bonuses
                    Debug.Log("Added " + talent.effectValue + " Strength");
                }
                break;

            case TalentEffectType.IncreaseVitality:
                if (playerStats != null)
                {
                    Debug.Log("Added " + talent.effectValue + " Vitality");
                }
                break;

            case TalentEffectType.WeaponDamageBonus:
                // Weapon mastery - will be calculated when dealing damage
                Debug.Log("Weapon damage bonus applied for " + talent.affectedWeaponClass);
                break;

            case TalentEffectType.WeaponSpeedBonus:
                // Weapon speed - will be calculated when equipping
                Debug.Log("Weapon speed bonus applied for " + talent.affectedWeaponClass);
                break;

                // Add other cases as needed
        }
    }

    public int GetTalentRank(TalentData talent)
    {
        if (learnedTalents.ContainsKey(talent))
        {
            return learnedTalents[talent];
        }
        return 0;
    }

    public int GetAvailableTalentPoints()
    {
        return availableTalentPoints;
    }

    public float GetWeaponMasteryDamageBonus(WeaponClass weaponClass)
    {
        float totalBonus = 0f;

        foreach (var kvp in learnedTalents)
        {
            TalentData talent = kvp.Key;
            int rank = kvp.Value;

            if (talent.effectType == TalentEffectType.WeaponDamageBonus &&
                talent.affectedWeaponClass == weaponClass)
            {
                totalBonus += talent.effectValue * rank;
            }
        }

        return totalBonus;
    }

    public float GetWeaponMasterySpeedBonus(WeaponClass weaponClass)
    {
        float totalBonus = 0f;

        foreach (var kvp in learnedTalents)
        {
            TalentData talent = kvp.Key;
            int rank = kvp.Value;

            if (talent.effectType == TalentEffectType.WeaponSpeedBonus &&
                talent.affectedWeaponClass == weaponClass)
            {
                totalBonus += talent.effectValue * rank;
            }
        }

        return totalBonus;
    }
}