using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Central manager for the action bar.
/// Handles input polling and routes key presses to the correct slot.
/// Lives on the PersistentUICanvas in GameBootstrap.
/// </summary>
public class ActionBar : MonoBehaviour
{
    public static ActionBar Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] private ActionBarSlot lmbSlot;
    [SerializeField] private ActionBarSlot rmbSlot;
    [SerializeField] private ActionBarSlot slot1;
    [SerializeField] private ActionBarSlot slot2;
    [SerializeField] private ActionBarSlot slot3;
    [SerializeField] private ActionBarSlot slot4;

    // Maps SlotType to slot for easy lookup
    private Dictionary<SlotType, ActionBarSlot> slots;

    // Maps SkillData to the ISkill component on the player that executes it
    private Dictionary<SkillData, ISkill> skillRegistry = new Dictionary<SkillData, ISkill>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        slots = new Dictionary<SlotType, ActionBarSlot>
        {
            { SlotType.LMB, lmbSlot },
            { SlotType.RMB, rmbSlot },
            { SlotType.Key1, slot1 },
            { SlotType.Key2, slot2 },
            { SlotType.Key3, slot3 },
            { SlotType.Key4, slot4 },
        };

        // LMB is always locked
        if (lmbSlot != null) lmbSlot.isLocked = true;
    }

    void Update()
    {
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsCombatInputBlocked())
            return;

        // LMB — held for continuous attack
        if (Input.GetMouseButton(0))
            lmbSlot?.TryExecute();

        // RMB — pressed
        if (Input.GetMouseButtonDown(1))
            rmbSlot?.TryExecute();

        // 1-4
        if (Input.GetKeyDown(KeyCode.Alpha1)) slot1?.TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha2)) slot2?.TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha3)) slot3?.TryExecute();
        if (Input.GetKeyDown(KeyCode.Alpha4)) slot4?.TryExecute();
    }

    /// <summary>
    /// Register a skill component so the action bar can execute it.
    /// Called by each ISkill implementation on Start().
    /// </summary>
    public void RegisterSkill(ISkill skill)
    {
        if (skill?.SkillData == null) return;
        skillRegistry[skill.SkillData] = skill;

        // If this skill is already assigned to a slot (e.g. after scene reload),
        // re-link the ISkill reference
        foreach (var kvp in slots)
        {
            ActionBarSlot slot = kvp.Value;
            if (slot.GetAssignedSkillData() == skill.SkillData)
                slot.AssignSkill(skill.SkillData, skill);
        }

        Debug.Log($"Skill registered: {skill.SkillData.skillName}");
    }

    /// <summary>
    /// Assign a skill to a slot by SlotType.
    /// Called by drag-and-drop from SpellbookUI, or on init for LMB.
    /// </summary>
    public void AssignSkillToSlot(SkillData skillData, SlotType slotType)
    {
        if (!slots.ContainsKey(slotType)) return;
        if (slotType == SlotType.LMB) return; // LMB is always Attack, locked

        // If this skill is already in another slot, clear that slot
        foreach (var kvp in slots)
        {
            if (kvp.Key != slotType && kvp.Value.GetAssignedSkillData() == skillData)
                kvp.Value.ClearSlot();
        }

        ISkill skill = skillRegistry.ContainsKey(skillData) ? skillRegistry[skillData] : null;
        slots[slotType].AssignSkill(skillData, skill);
    }

    /// <summary>
    /// Called by MeleeSkill on Start to lock itself into LMB.
    /// </summary>
    public void AssignAttackToLMB(SkillData skillData, ISkill skill)
    {
        skillRegistry[skillData] = skill;
        lmbSlot?.AssignSkill(skillData, skill);
    }

    public ActionBarSlot GetSlot(SlotType slotType)
    {
        return slots.ContainsKey(slotType) ? slots[slotType] : null;
    }
}