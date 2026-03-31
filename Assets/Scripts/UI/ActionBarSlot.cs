using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// A single slot on the action bar.
/// Handles cooldown display, input label, and skill assignment.
/// </summary>
public class ActionBarSlot : MonoBehaviour, IDropHandler
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;   // Radial fill image, set Fill Method to Radial360
    [SerializeField] private Image borderImage;
    [SerializeField] private TextMeshProUGUI keyLabel; // Shows "LMB", "RMB", "1", etc.
    [SerializeField] private GameObject emptySlotIndicator;

    [Header("Slot Config")]
    public SlotType slotType;
    public bool isLocked = false; // LMB is locked to Attack

    // The skill currently assigned to this slot
    private SkillData assignedSkillData;
    private ISkill assignedSkill;

    // Colors
    private static readonly Color ReadyColor = new Color(0f, 0f, 0f, 0f);
    private static readonly Color CooldownColor = new Color(0f, 0f, 0f, 0.7f);

    void Awake()
    {
        SetKeyLabel();
        SetEmpty();
    }

    void Update()
    {
        if (assignedSkill == null) return;

        float cooldownPercent = assignedSkill.GetCooldownPercent();

        if (cooldownOverlay != null)
        {
            // fillAmount: 1 = full cooldown overlay, 0 = ready
            cooldownOverlay.fillAmount = cooldownPercent;
            cooldownOverlay.color = cooldownPercent > 0f ? CooldownColor : ReadyColor;
        }
    }

    /// <summary>
    /// Assign a skill to this slot. Pass null to clear it.
    /// </summary>
    public void AssignSkill(SkillData skillData, ISkill skill)
    {
        assignedSkillData = skillData;
        assignedSkill = skill;

        if (skillData != null && skillData.icon != null)
        {
            if (iconImage != null) iconImage.sprite = skillData.icon;
            if (iconImage != null) iconImage.color = Color.white;
            if (emptySlotIndicator != null) emptySlotIndicator.SetActive(false);
        }
        else
        {
            SetEmpty();
        }
    }

    public void ClearSlot()
    {
        if (isLocked) return;
        AssignSkill(null, null);
    }

    public SkillData GetAssignedSkillData() => assignedSkillData;
    public ISkill GetAssignedSkill() => assignedSkill;
    public bool HasSkill() => assignedSkill != null;

    /// <summary>
    /// Called by the action bar when the corresponding key is pressed.
    /// </summary>
    public void TryExecute()
    {
        if (assignedSkill == null) return;
        if (!assignedSkill.CanExecute()) return;
        assignedSkill.Execute();
    }

    void SetEmpty()
    {
        if (iconImage != null) iconImage.color = new Color(1f, 1f, 1f, 0f);
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (emptySlotIndicator != null) emptySlotIndicator.SetActive(true);
    }

    void SetKeyLabel()
    {
        if (keyLabel == null) return;
        keyLabel.text = slotType switch
        {
            SlotType.LMB => "LMB",
            SlotType.RMB => "RMB",
            SlotType.Key1 => "1",
            SlotType.Key2 => "2",
            SlotType.Key3 => "3",
            SlotType.Key4 => "4",
            _ => ""
        };
    }

    // IDropHandler — called when a skill icon is dropped onto this slot from the spellbook
    public void OnDrop(PointerEventData eventData)
    {
        if (isLocked) return;

        SpellbookDraggable draggable = eventData.pointerDrag?.GetComponent<SpellbookDraggable>();
        if (draggable == null) return;

        ActionBar.Instance?.AssignSkillToSlot(draggable.SkillData, slotType);
    }
}

public enum SlotType
{
    LMB,
    RMB,
    Key1,
    Key2,
    Key3,
    Key4
}