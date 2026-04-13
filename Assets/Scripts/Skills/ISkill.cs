/// <summary>
/// Implement this on any MonoBehaviour that represents an executable skill.
/// The action bar calls Execute() and checks CanExecute() — it knows nothing
/// about what the skill actually does.
/// </summary>
public interface ISkill
{
    /// <summary>The SkillData asset that describes this skill.</summary>
    SkillData SkillData { get; }

    /// <summary>Returns true if the skill can currently be used.</summary>
    bool CanExecute();

    /// <summary>Executes the skill.</summary>
    void Execute();

    /// <summary>
    /// 0 = ready, 1 = full cooldown. Used by the action bar radial fill.
    /// </summary>
    float GetCooldownPercent();
    /// <summary>True while the skill is mid-execution (animation playing, etc.)</summary>
    bool IsActive();
}