using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class TalentNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private TalentData talentData;
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TextMeshProUGUI talentNameText;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Colors")]
    [SerializeField] private Color availableColor = new Color(0.2f, 0.8f, 0.2f); // Green
    [SerializeField] private Color lockedColor = new Color(0.3f, 0.3f, 0.3f); // Gray
    [SerializeField] private Color learnedColor = new Color(0.8f, 0.6f, 0.2f); // Gold
    [SerializeField] private Color maxedColor = new Color(1f, 0.8f, 0f); // Bright Gold

    private TalentTreeUI talentTreeUI;
    private PlayerTalents playerTalents;

    public void Initialize(TalentData talent, TalentTreeUI treeUI, PlayerTalents talents)
    {
        talentData = talent;
        talentTreeUI = treeUI;
        playerTalents = talents;

        if (button == null) button = GetComponent<Button>();
        if (background == null) background = GetComponent<Image>();

        // Set up button click
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnNodeClicked);
        }

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (talentData == null || playerTalents == null) return;

        int currentRank = playerTalents.GetTalentRank(talentData);
        int maxRank = talentData.maxRank;
        bool canLearn = playerTalents.CanLearnTalent(talentData);

        // Update name
        if (talentNameText != null)
        {
            talentNameText.text = talentData.talentName;
        }

        // Update rank display
        if (rankText != null)
        {
            rankText.text = currentRank + "/" + maxRank;
        }

        // Update visual state
        if (background != null)
        {
            if (currentRank >= maxRank)
            {
                // Maxed out
                background.color = maxedColor;
            }
            else if (currentRank > 0)
            {
                // Partially learned
                background.color = learnedColor;
            }
            else if (canLearn)
            {
                // Available to learn
                background.color = availableColor;
            }
            else
            {
                // Locked (prerequisites not met)
                background.color = lockedColor;
            }
        }

        // Update button interactability
        if (button != null)
        {
            button.interactable = canLearn;
        }
    }

    void OnNodeClicked()
    {
        Debug.Log("Node clicked! Talent: " + (talentData != null ? talentData.talentName : "NULL"));

        if (playerTalents == null)
        {
            Debug.LogError("PlayerTalents is NULL!");
            return;
        }

        if (talentData == null)
        {
            Debug.LogError("TalentData is NULL!");
            return;
        }

        Debug.Log("Can learn? " + playerTalents.CanLearnTalent(talentData));
        Debug.Log("Available points: " + playerTalents.GetAvailableTalentPoints());

        if (playerTalents.CanLearnTalent(talentData))
        {
            playerTalents.LearnTalent(talentData);

            // Update this node
            UpdateDisplay();

            // Refresh tooltip immediately (if it's currently showing)
            if (talentTreeUI != null)
            {
                talentTreeUI.RefreshAllNodes();
                // Update tooltip with new values
                talentTreeUI.ShowTooltip(talentData, playerTalents);
            }
        }
        else
        {
            Debug.Log("Cannot learn talent - prerequisites not met or no points available");
        }
    }

    // Tooltip on hover
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (talentTreeUI != null && talentData != null)
        {
            talentTreeUI.ShowTooltip(talentData, playerTalents);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (talentTreeUI != null)
        {
            talentTreeUI.HideTooltip();
        }
    }

    public TalentData GetTalentData()
    {
        return talentData;
    }

    public void SetTalentData(TalentData talent)
    {
        talentData = talent;
    }
}