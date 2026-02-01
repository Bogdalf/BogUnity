using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TalentTreeUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerTalents playerTalents;
    [SerializeField] private GameObject talentTreePanel;
    [SerializeField] private TextMeshProUGUI talentPointsText;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipTitleText;
    [SerializeField] private TextMeshProUGUI tooltipDescriptionText;
    [SerializeField] private TextMeshProUGUI tooltipRankText;
    [SerializeField] private TextMeshProUGUI tooltipEffectText;

    [Header("Talent Nodes")]
    [SerializeField] private TalentNodeUI[] talentNodes;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.N;

    private bool isTalentTreeOpen = false;

    public bool IsTalentTreeOpen()
    {
        return isTalentTreeOpen;
    }

    void Start()
    {
        // Find player reference if not set
        FindPlayerReferences();

        // Hide tooltip initially
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }

        // Initialize all nodes
        InitializeAllNodes();
        RefreshAllNodes();
    }

    void FindPlayerReferences()
    {
        // Try to find via PersistentPlayer first
        if (PersistentPlayer.Instance != null)
        {
            if (playerTalents == null)
            {
                playerTalents = PersistentPlayer.Instance.GetComponent<PlayerTalents>();
                if (playerTalents != null)
                {
                    Debug.Log("TalentTreeUI: Found PlayerTalents on PersistentPlayer");
                }
            }
        }
        else
        {
            // Fallback - find by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && playerTalents == null)
            {
                playerTalents = player.GetComponent<PlayerTalents>();
            }
        }

        if (playerTalents == null)
        {
            Debug.LogWarning("TalentTreeUI: Could not find PlayerTalents component!");
        }
    }

    void InitializeAllNodes()
    {
        if (playerTalents == null) return;

        foreach (TalentNodeUI node in talentNodes)
        {
            if (node != null)
            {
                TalentData talent = node.GetTalentData();
                node.Initialize(talent, this, playerTalents);
            }
        }
    }

    void Update()
    {
        // If player talents is null, try to find it again
        if (playerTalents == null)
        {
            FindPlayerReferences();
        }

        // Toggle talent tree with N key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleTalentTree();
        }

        // Update talent points display if tree is open
        if (isTalentTreeOpen)
        {
            UpdateTalentPointsDisplay();
        }
    }

    void ToggleTalentTree()
    {
        isTalentTreeOpen = !isTalentTreeOpen;

        if (talentTreePanel != null)
        {
            talentTreePanel.SetActive(isTalentTreeOpen);
        }

        if (isTalentTreeOpen)
        {
            RefreshAllNodes();
            UpdateTalentPointsDisplay();
        }
        else
        {
            HideTooltip();
        }
    }

    public void RefreshAllNodes()
    {
        if (playerTalents == null) return;

        foreach (TalentNodeUI node in talentNodes)
        {
            if (node != null)
            {
                node.UpdateDisplay();
            }
        }

        UpdateTalentPointsDisplay();
    }

    void UpdateTalentPointsDisplay()
    {
        if (talentPointsText != null && playerTalents != null)
        {
            int availablePoints = playerTalents.GetAvailableTalentPoints();
            talentPointsText.text = "Available Points: " + availablePoints;
        }
    }

    public void ShowTooltip(TalentData talent, PlayerTalents talents)
    {
        if (tooltipPanel == null || talent == null) return;

        tooltipPanel.SetActive(true);

        // Title
        if (tooltipTitleText != null)
        {
            tooltipTitleText.text = talent.talentName;
        }

        // Description
        if (tooltipDescriptionText != null)
        {
            tooltipDescriptionText.text = talent.talentDescription;
        }

        // Rank info
        if (tooltipRankText != null && talents != null)
        {
            int currentRank = talents.GetTalentRank(talent);
            tooltipRankText.text = "Rank: " + currentRank + "/" + talent.maxRank;
        }

        // Effect info
        if (tooltipEffectText != null)
        {
            string effectText = GetEffectDescription(talent);
            tooltipEffectText.text = effectText;
        }

        // Position tooltip near mouse (optional - you can adjust this)
        Vector3 mousePos = Input.mousePosition;
        tooltipPanel.transform.position = mousePos + new Vector3(100, -50, 0);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    string GetEffectDescription(TalentData talent)
    {
        // Get current rank to calculate total effect
        int currentRank = 0;
        if (playerTalents != null)
        {
            currentRank = playerTalents.GetTalentRank(talent);
        }

        // Calculate total effect (base value * current rank)
        float totalEffect = talent.effectValue * Mathf.Max(1, currentRank);

        switch (talent.effectType)
        {
            case TalentEffectType.IncreaseStrength:
                return "+" + totalEffect + " Strength";

            case TalentEffectType.IncreaseVitality:
                return "+" + totalEffect + " Vitality";

            case TalentEffectType.WeaponDamageBonus:
                return "+" + totalEffect + "% " + talent.affectedWeaponClass + " Damage";

            case TalentEffectType.WeaponSpeedBonus:
                return "+" + totalEffect + "% " + talent.affectedWeaponClass + " Attack Speed";

            case TalentEffectType.IncreaseDashDistance:
                return "+" + totalEffect + "% Dash Distance";

            case TalentEffectType.DecreaseDashCooldown:
                return "-" + totalEffect + "% Dash Cooldown";

            case TalentEffectType.IncreaseChargeDistance:
                return "+" + totalEffect + "% Charge Distance";

            case TalentEffectType.DecreaseChargeCooldown:
                return "-" + totalEffect + "% Charge Cooldown";

            default:
                return talent.effectType.ToString();
        }
    }
}