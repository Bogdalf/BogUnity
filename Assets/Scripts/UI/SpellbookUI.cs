using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the player's unlocked skills and allows drag-and-drop
/// assignment to action bar slots.
/// Toggle with P key.
/// </summary>
public class SpellbookUI : MonoBehaviour, IUIPanel
{
    [Header("References")]
    [SerializeField] private GameObject spellbookPanel;
    [SerializeField] private Transform skillGridParent;
    [SerializeField] private GameObject skillEntryPrefab;

    private PlayerSkillbook skillbook;
    private bool isOpen = false;

    void Start()
    {
        if (spellbookPanel != null)
            spellbookPanel.SetActive(false);

        skillbook = FindFirstObjectByType<PlayerSkillbook>();
        if (skillbook != null)
            skillbook.OnSkillsChanged += RefreshSkillList;

        RefreshSkillList();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.P)) return;

        if (isOpen)
            Close();
        else
            Open();
    }

    // ─── Open / Close ─────────────────────────────────────────────────────────────

    void Open()
    {
        isOpen = true;
        UIPanelManager.Instance?.OnPanelOpening(this, UIPanelManager.PanelRegion.Center);
        spellbookPanel?.SetActive(true);
        RefreshSkillList();
    }

    public void Close()
    {
        isOpen = false;
        spellbookPanel?.SetActive(false);
        UIPanelManager.Instance?.OnPanelClosed(this, UIPanelManager.PanelRegion.Center);
    }

    // ─── Skill List ───────────────────────────────────────────────────────────────

    void RefreshSkillList()
    {
        if (skillGridParent == null || skillEntryPrefab == null) return;

        foreach (Transform child in skillGridParent)
            Destroy(child.gameObject);

        if (skillbook == null) return;

        foreach (SkillData skill in skillbook.GetUnlockedSkills())
        {
            if (!skill.showInSpellbook) continue;

            GameObject entry = Instantiate(skillEntryPrefab, skillGridParent);

            SpellbookDraggable draggable = entry.GetComponent<SpellbookDraggable>();
            draggable?.Initialize(skill);

            TextMeshProUGUI label = entry.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null) label.text = skill.skillName;
        }
    }

    void OnDestroy()
    {
        if (skillbook != null)
            skillbook.OnSkillsChanged -= RefreshSkillList;
    }

    public bool IsOpen() => isOpen;
}