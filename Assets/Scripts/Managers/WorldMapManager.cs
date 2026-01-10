using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class WorldMapManager : MonoBehaviour
{
    public static WorldMapManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private HexGrid hexGrid;

    [Header("UI Panel")]
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private TextMeshProUGUI missionNameText;
    [SerializeField] private TextMeshProUGUI missionDescriptionText;
    [SerializeField] private TextMeshProUGUI missionTypeText;
    [SerializeField] private TextMeshProUGUI recommendedLevelText;
    [SerializeField] private TextMeshProUGUI rewardsText;
    [SerializeField] private Button startMissionButton;
    [SerializeField] private Button closePanelButton;

    private HexNode currentlySelectedHex;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Hide mission panel initially
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        // Setup button listeners
        if (startMissionButton != null)
        {
            startMissionButton.onClick.AddListener(OnStartMissionClicked);
        }

        if (closePanelButton != null)
        {
            closePanelButton.onClick.AddListener(OnClosePanelClicked);
        }
    }

    public void SelectHex(HexNode hexNode)
    {
        // Deselect previous hex
        if (currentlySelectedHex != null)
        {
            currentlySelectedHex.SetSelected(false);
        }

        // Select new hex
        currentlySelectedHex = hexNode;
        currentlySelectedHex.SetSelected(true);

        // Show mission panel with hex data
        ShowMissionPanel(hexNode);
    }

    private void ShowMissionPanel(HexNode hexNode)
    {
        if (missionPanel == null) return;

        missionPanel.SetActive(true);

        // If hex has mission data, display it
        if (hexNode.missionData != null)
        {
            MissionData mission = hexNode.missionData;

            if (missionNameText != null)
                missionNameText.text = mission.missionName;

            if (missionDescriptionText != null)
                missionDescriptionText.text = mission.description;

            if (missionTypeText != null)
                missionTypeText.text = $"Type: {mission.missionType}";

            if (recommendedLevelText != null)
                recommendedLevelText.text = $"Recommended Level: {mission.recommendedLevel}";

            if (rewardsText != null)
            {
                string rewards = $"Rewards:\n{mission.rewardDescription}";
                if (mission.experienceReward > 0)
                    rewards += $"\n+{mission.experienceReward} XP";
                if (mission.goldReward > 0)
                    rewards += $"\n+{mission.goldReward} Gold";
                rewardsText.text = rewards;
            }

            // Enable/disable start button based on lock status
            if (startMissionButton != null)
            {
                startMissionButton.interactable = !mission.isLocked;
            }
        }
        else
        {
            // No mission data - show placeholder
            if (missionNameText != null)
                missionNameText.text = $"Location ({hexNode.q}, {hexNode.r})";

            if (missionDescriptionText != null)
                missionDescriptionText.text = "No mission data assigned to this hex.";

            if (missionTypeText != null)
                missionTypeText.text = "";

            if (recommendedLevelText != null)
                recommendedLevelText.text = "";

            if (rewardsText != null)
                rewardsText.text = "";

            if (startMissionButton != null)
            {
                startMissionButton.interactable = false;
            }
        }
    }

    private void OnStartMissionClicked()
    {
        if (currentlySelectedHex == null || currentlySelectedHex.missionData == null)
        {
            Debug.LogWarning("Cannot start mission: No hex selected or no mission data.");
            return;
        }

        MissionData mission = currentlySelectedHex.missionData;

        if (mission.isLocked)
        {
            Debug.LogWarning($"Mission '{mission.missionName}' is locked!");
            return;
        }

        if (string.IsNullOrEmpty(mission.sceneName))
        {
            Debug.LogWarning($"Mission '{mission.missionName}' has no scene assigned!");
            return;
        }

        // TODO: Save current hex as player location
        // TODO: Save any necessary game state

        Debug.Log($"Starting mission: {mission.missionName}, loading scene: {mission.sceneName}");

        // Load the mission scene
        SceneManager.LoadScene(mission.sceneName);
    }

    private void OnClosePanelClicked()
    {
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        if (currentlySelectedHex != null)
        {
            currentlySelectedHex.SetSelected(false);
            currentlySelectedHex = null;
        }
    }

    // Public method to set player's current location
    public void SetPlayerLocation(int q, int r)
    {
        HexNode hex = hexGrid.GetHexAt(q, r);
        if (hex != null)
        {
            // Clear any previous current location
            foreach (HexNode node in hexGrid.GetAllHexes())
            {
                if (node.GetState() == HexNode.HexState.CurrentLocation)
                {
                    node.SetState(HexNode.HexState.Completed);
                }
            }

            hex.SetState(HexNode.HexState.CurrentLocation);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}