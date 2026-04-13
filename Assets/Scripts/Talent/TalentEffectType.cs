/// <summary>
/// Defines what effect a talent node applies when learned.
/// </summary>
public enum TalentEffectType
{
    // ── Core Stat Bonuses ────────────────────────────────────────
    IncreaseStrength,
    IncreaseIntelligence,
    IncreaseFocus,
    IncreaseVitality,

    // ── Aspect Power Bonuses ─────────────────────────────────────
    WarDamageBonus,
    SorceryDamageBonus,
    AmplificationBonus,

    // ── Ability Modifications ────────────────────────────────────
    IncreaseDashDistance,
    DecreaseDashCooldown,
    IncreaseMeleeRange,
    IncreaseMeleeArc,
}