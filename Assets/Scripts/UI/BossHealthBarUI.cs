using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays the active boss's health bar and name.
/// Hidden when no boss is active.
///
/// Setup: Add to PersistentUICanvas alongside other HUD elements.
/// Position at the top-center of the screen (common ARPG convention).
///
/// The bar auto-finds the active boss via BossHealthBarUI.RegisterBoss()
/// which is called by BossBase.TriggerEntrance().
/// </summary>
public class BossHealthBarUI : MonoBehaviour
{
    public static BossHealthBarUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bossHealthBarPanel;
    [SerializeField] private Image      healthBarFill;
    [SerializeField] private Image      healthBarBackground;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private TextMeshProUGUI healthValueText;

    [Header("Colors")]
    [SerializeField] private Color healthColorHigh   = new Color(0.8f, 0.1f, 0.1f); // Red
    [SerializeField] private Color healthColorMedium = new Color(0.9f, 0.5f, 0.1f); // Orange
    [SerializeField] private Color healthColorLow    = new Color(1f,   0.9f, 0.1f); // Yellow

    [Header("Phase Indicator (Optional)")]
    [SerializeField] private TextMeshProUGUI phaseText;

    private BossBase activeBoss;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        // Hidden until a boss registers
        if (bossHealthBarPanel != null)
            bossHealthBarPanel.SetActive(false);
    }

    void Update()
    {
        if (activeBoss == null || activeBoss.IsDead)
        {
            HideBar();
            return;
        }

        UpdateBar();
    }

    // ─── Registration ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by BossBase.TriggerEntrance() to register the active boss.
    /// </summary>
    public void RegisterBoss(BossBase boss, string bossName)
    {
        activeBoss = boss;

        if (bossNameText != null)
            bossNameText.text = bossName;

        if (phaseText != null)
            phaseText.text = "";

        if (bossHealthBarPanel != null)
            bossHealthBarPanel.SetActive(true);

        UpdateBar();
        Debug.Log($"Boss health bar registered: {bossName}");
    }

    public void UnregisterBoss()
    {
        activeBoss = null;
        HideBar();
    }

    // ─── Update ───────────────────────────────────────────────────────────────────

    void UpdateBar()
    {
        if (activeBoss == null) return;

        float percent = activeBoss.GetHealthPercent();

        // Fill amount
        if (healthBarFill != null)
            healthBarFill.fillAmount = percent;

        // Color shifts based on health
        if (healthBarFill != null)
        {
            if (percent > 0.66f)
                healthBarFill.color = healthColorHigh;
            else if (percent > 0.33f)
                healthBarFill.color = healthColorMedium;
            else
                healthBarFill.color = healthColorLow;
        }

        // Health value text
        if (healthValueText != null)
        {
            float textpercent = activeBoss.GetHealthPercent() * 100f;
            healthValueText.text = $"{Mathf.Ceil(textpercent)}%";
        }

        // Phase indicator
        if (phaseText != null)
            phaseText.text = activeBoss.CurrentPhase > 1 ? $"Phase {activeBoss.CurrentPhase}" : "";
    }

    void HideBar()
    {
        if (bossHealthBarPanel != null && bossHealthBarPanel.activeSelf)
            bossHealthBarPanel.SetActive(false);

        activeBoss = null;
    }
}