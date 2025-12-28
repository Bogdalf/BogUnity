using UnityEngine;

[CreateAssetMenu(fileName = "NewTalent", menuName = "Talents/Talent")]
public class TalentData : ScriptableObject
{
    [Header("Talent Info")]
    public string talentName;
    public string talentDescription;
    public int maxRank = 1; // How many times can this be taken

    [Header("Requirements")]
    public int requiredLevel = 0;
    public TalentData prerequisiteTalent; // Must have this talent first

    [Header("Effects")]
    public TalentEffectType effectType;
    public float effectValue;
    public WeaponClass affectedWeaponClass = WeaponClass.None; // For weapon mastery talents
}

public enum TalentEffectType
{
    // Stat bonuses
    IncreaseStrength,
    IncreaseVitality,

    // Weapon masteries
    WeaponDamageBonus,      // +X% damage with specific weapon class
    WeaponSpeedBonus,       // -X% cooldown with specific weapon class

    // Ability modifications
    IncreaseDashDistance,
    DecreaseDashCooldown,
    IncreaseChargeDistance,
    DecreaseChargeCooldown,
    IncreaseMeleeRange,
    IncreaseMeleeArc,

    // Special effects
    DualWieldExtraHit,
    TwoHandedCritChance,
    ShieldBlock,
    AxeFrenzy,              // NEW: Stacking attack speed buff on axe hits
}