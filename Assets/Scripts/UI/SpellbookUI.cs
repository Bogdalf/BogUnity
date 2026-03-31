using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Spellbook panel — opened/closed with P.
/// Displays all skills in PlayerSkillbook as draggable icons.
/// </summary>
public class SpellbookUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject spellbookPanel;
    [SerializeField] private Transform skillGridParent;    // Grid layout group parent
    [SerializeField] private GameObject skillEntryPrefab; // Prefab with SpellbookDraggable + Image + TextMeshProUGUI

    private PlayerSkillbook skillbook;
    private bool isOpen = false;

    void Start()
    {
        if (spellbookPanel != null)
            spellbookPanel.SetActive(false);

        // Find skillbook on the persistent player
        skillbook = FindFirstObjectByType<PlayerSkillbook>();
        if (skillbook != null)
            skillbook.OnSkillsChanged += RefreshSkillList;

        RefreshSkillList();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            ToggleSpellbook();
    }

    void ToggleSpellbook()
    {
        isOpen = !isOpen;
        spellbookPanel?.SetActive(isOpen);

        // Block/unblock input while spellbook is open
        if (PersistentInputManager.Instance != null)
            PersistentInputManager.Instance.SetSpellbookOpen(isOpen);
    }

    void RefreshSkillList()
    {
        if (skillGridParent == null || skillEntryPrefab == null) return;

        // Clear existing entries
        foreach (Transform child in skillGridParent)
            Destroy(child.gameObject);

        if (skillbook == null) return;

        foreach (SkillData skill in skillbook.GetUnlockedSkills())
        {
            GameObject entry = Instantiate(skillEntryPrefab, skillGridParent);

            // Set icon
            SpellbookDraggable draggable = entry.GetComponent<SpellbookDraggable>();
            draggable?.Initialize(skill);

            // Set name label
            TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = skill.skillName;

            // Set tooltip (optional — can expand later)
            // entry.GetComponent<SkillTooltip>()?.Initialize(skill);
        }
    }

    void OnDestroy()
    {
        if (skillbook != null)
            skillbook.OnSkillsChanged -= RefreshSkillList;
    }

    public bool IsOpen() => isOpen;
}