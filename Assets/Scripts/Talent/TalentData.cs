using UnityEngine;

[CreateAssetMenu(fileName = "NewTalent", menuName = "Talents/Talent")]
public class TalentData : ScriptableObject
{
    [Header("Talent Info")]
    public string talentName;
    public string talentDescription;
    public int maxRank = 1;

    [Header("Requirements")]
    public int requiredLevel = 0;
    public TalentData prerequisiteTalent;

    [Header("Effects")]
    public TalentEffectType effectType;
    public float effectValue;
    public WeaponClass affectedWeaponClass = WeaponClass.None;
}

public enum TalentEffectType
{
    // Stat bonuses
    IncreaseStrength,
    IncreaseVitality,

    // Weapon masteries
    WeaponDamageBonus,
    WeaponSpeedBonus,

    // Ability modifications
    IncreaseDashDistance,
    DecreaseDashCooldown,
    IncreaseMeleeRange,
    IncreaseMeleeArc,

    // Special effects
    DualWieldExtraHit,
    TwoHandedCritChance,
    ShieldBlock,
    AxeFrenzy,
}