using UnityEngine;
using TMPro;

/// <summary>
/// Displays a simple character stat sheet panel.
/// Toggle with C key (or whatever key you assign).
/// Reads live values from PlayerStats each time the panel opens.
/// </summary>
public class CharacterStatSheet : MonoBehaviour, IUIPanel
{
    [Header("Panel")]
    [SerializeField] private GameObject statSheetPanel;

    [Header("Stat Labels")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI maxHPText;
    [SerializeField] private TextMeshProUGUI strengthText;
    [SerializeField] private TextMeshProUGUI intelligenceText;
    [SerializeField] private TextMeshProUGUI focusText;
    [SerializeField] private TextMeshProUGUI vitalityText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.C;

    private PlayerStats playerStats;
    private PlayerHealth playerHealth;
    private bool isOpen = false;

    void Start()
    {
        FindPlayerReferences();

        if (statSheetPanel != null)
            statSheetPanel.SetActive(false);
    }

    void Update()
    {
        if (playerStats == null)
            FindPlayerReferences();

        if (!Input.GetKeyDown(toggleKey)) return;

        if (isOpen)
        {
            Close();
            return;
        }

        if (PersistentInputManager.Instance != null &&
            PersistentInputManager.Instance.IsPlayerInputBlocked()) return;

        Open();
    }

    void FindPlayerReferences()
    {
        GameObject player = PersistentPlayer.Instance != null
            ? PersistentPlayer.Instance.gameObject
            : GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        playerStats  = player.GetComponent<PlayerStats>();
        playerHealth = player.GetComponent<PlayerHealth>();
    }

    // ─── Open / Close ─────────────────────────────────────────────────────────────

    void Open()
    {
        isOpen = true;
        UIPanelManager.Instance?.OnPanelOpening(this, UIPanelManager.PanelRegion.Center);
        statSheetPanel?.SetActive(true);
        RefreshStats();
    }

    public void Close()
    {
        isOpen = false;
        statSheetPanel?.SetActive(false);
        UIPanelManager.Instance?.OnPanelClosed(this, UIPanelManager.PanelRegion.Center);
    }

    // ─── Refresh ──────────────────────────────────────────────────────────────────

    void RefreshStats()
    {
        if (playerStats == null) return;

        if (levelText != null)
            levelText.text = "Level: --";

        if (maxHPText != null)
            maxHPText.text = $"Maximum HP: {playerStats.GetMaxHealth():0}";

        if (strengthText != null)
            strengthText.text = $"Strength: {playerStats.GetStrength():0.#}";

        if (intelligenceText != null)
            intelligenceText.text = $"Intelligence: {playerStats.GetIntelligence():0.#}";

        if (focusText != null)
            focusText.text = $"Focus: {playerStats.GetFocus():0.#}";

        if (vitalityText != null)
            vitalityText.text = $"Vitality: {playerStats.GetVitality():0.#}";
    }

    public bool IsOpen() => isOpen;
}