using UnityEngine;
using System.Collections.Generic;

public class PlayerTalents : MonoBehaviour
{
    [Header("Talent Points")]
    [SerializeField] private int availableTalentPoints = 10;

    private Dictionary<TalentData, int> learnedTalents = new Dictionary<TalentData, int>();

    private PlayerStats playerStats;
    private PlayerEquipment playerEquipment;
    private PlayerMelee playerMelee;
    private PlayerDash playerDash;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerEquipment = GetComponent<PlayerEquipment>();
        playerMelee = GetComponent<PlayerMelee>();
        playerDash = GetComponent<PlayerDash>();
    }

    public bool CanLearnTalent(TalentData talent)
    {
        if (talent == null) return false;
        if (availableTalentPoints <= 0) return false;

        int currentRank = GetTalentRank(talent);
        if (currentRank >= talent.maxRank) return false;

        if (talent.prerequisiteTalent != null && GetTalentRank(talent.prerequisiteTalent) <= 0)
        {
            Debug.Log("Missing prerequisite: " + talent.prerequisiteTalent.talentName);
            return false;
        }

        return true;
    }

    public void LearnTalent(TalentData talent)
    {
        if (!CanLearnTalent(talent)) return;

        if (learnedTalents.ContainsKey(talent))
            learnedTalents[talent]++;
        else
            learnedTalents[talent] = 1;

        availableTalentPoints--;
        ApplyTalentEffect(talent);

        Debug.Log("Learned: " + talent.talentName + " (Rank " + learnedTalents[talent] + ")");
    }

    void ApplyTalentEffect(TalentData talent)
    {
        switch (talent.effectType)
        {
            case TalentEffectType.IncreaseStrength:
                if (playerStats != null) playerStats.AddStrength(talent.effectValue);
                break;

            case TalentEffectType.IncreaseVitality:
                if (playerStats != null) playerStats.AddVitality(talent.effectValue);
                break;

            case TalentEffectType.WeaponDamageBonus:
            case TalentEffectType.WeaponSpeedBonus:
                // Applied dynamically during damage/speed calculation
                if (playerEquipment != null) playerEquipment.RecalculateStats();
                break;

            case TalentEffectType.IncreaseDashDistance:
            case TalentEffectType.DecreaseDashCooldown:
                // PlayerDash reads talent values directly when needed
                break;

            case TalentEffectType.AxeFrenzy:
                // Passive — triggered by PlayerBuffs on axe hit
                break;

            default:
                Debug.Log("Talent effect not yet implemented: " + talent.effectType);
                break;
        }
    }

    public int GetTalentRank(TalentData talent)
    {
        if (talent == null) return 0;
        return learnedTalents.ContainsKey(talent) ? learnedTalents[talent] : 0;
    }

    public int GetAvailableTalentPoints() => availableTalentPoints;

    public float GetWeaponMasteryDamageBonus(WeaponClass weaponClass)
    {
        float total = 0f;
        foreach (var kvp in learnedTalents)
        {
            if (kvp.Key.effectType == TalentEffectType.WeaponDamageBonus &&
                kvp.Key.affectedWeaponClass == weaponClass)
            {
                total += kvp.Key.effectValue * kvp.Value;
            }
        }
        return total;
    }

    public float GetWeaponMasterySpeedBonus(WeaponClass weaponClass)
    {
        float total = 0f;
        foreach (var kvp in learnedTalents)
        {
            if (kvp.Key.effectType == TalentEffectType.WeaponSpeedBonus &&
                kvp.Key.affectedWeaponClass == weaponClass)
            {
                total += kvp.Key.effectValue * kvp.Value;
            }
        }
        return total;
    }
}