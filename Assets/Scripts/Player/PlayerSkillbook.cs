using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Tracks all skills the player has unlocked.
/// The spellbook UI reads from this to display available skills.
/// Add skills here when the player learns them (aspect choice, level up, etc.)
/// </summary>
public class PlayerSkillbook : MonoBehaviour
{
    [Header("Starting Skills")]
    [SerializeField] private List<SkillData> startingSkills = new List<SkillData>();

    private List<SkillData> unlockedSkills = new List<SkillData>();

    // Event so SpellbookUI can refresh when skills are added
    public event System.Action OnSkillsChanged;

    void Start()
    {
        foreach (SkillData skill in startingSkills)
            UnlockSkill(skill);
    }

    /// <summary>
    /// Grant the player a new skill. Called by aspect selection, talents, etc.
    /// </summary>
    public void UnlockSkill(SkillData skill)
    {
        if (skill == null || HasSkill(skill)) return;

        unlockedSkills.Add(skill);
        OnSkillsChanged?.Invoke();

        Debug.Log($"Skill unlocked: {skill.skillName}");
    }

    public bool HasSkill(SkillData skill)
    {
        return unlockedSkills.Contains(skill);
    }

    public List<SkillData> GetUnlockedSkills()
    {
        return new List<SkillData>(unlockedSkills);
    }
}