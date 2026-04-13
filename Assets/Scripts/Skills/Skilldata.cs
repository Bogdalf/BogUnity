using UnityEngine;

/// <summary>
/// ScriptableObject representing a single ability.
/// Create via Assets > Skills > New Skill.
/// </summary>
[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill")]
public class SkillData : ScriptableObject
{
    [Header("Identity")]
    public string skillName = "Unnamed Skill";
    [TextArea(2, 4)]
    public string description = "";
    public Sprite icon;

    [Header("Cooldown")]
    public float cooldown = 1f;

    [Header("Aspect")]
    public AspectType requiredAspect = AspectType.None; // None = available to all
    [Header("Spellbook")]
    [Tooltip("Uncheck for abilities that are permanently assigned and shouldn't appear in the spellbook.")]
    public bool showInSpellbook = true;
}

public enum AspectType
{
    None,
    War,
    Sorcery,
    Amplification
}