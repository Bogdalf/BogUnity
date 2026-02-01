using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages town-specific systems: buildings, NPCs, and town state.
/// This is scene-specific and does NOT persist across scenes.
/// </summary>
public class TownManager : MonoBehaviour
{
    public static TownManager Instance { get; private set; }

    [Header("Building Management")]
    [SerializeField] private List<BuildingPlot> buildingPlots = new List<BuildingPlot>();

    [Header("References")]
    [SerializeField] private Transform buildingsContainer; // Parent for instantiated buildings

    void Awake()
    {
        // Singleton for this scene only (gets destroyed when scene unloads)
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

    void Start()
    {
        InitializeTown();
    }

    void InitializeTown()
    {
        Debug.Log("Town initialized");

        // Find all building plots in the scene
        FindAllBuildingPlots();

        // Load town state from GameStateManager
        LoadTownState();
    }

    void FindAllBuildingPlots()
    {
        // Find all BuildingPlot components in the scene
        BuildingPlot[] foundPlots = FindObjectsByType<BuildingPlot>(FindObjectsSortMode.None);
        buildingPlots = new List<BuildingPlot>(foundPlots);

        Debug.Log($"Found {buildingPlots.Count} building plots in town");
    }

    void LoadTownState()
    {
        if (GameStateManager.Instance == null)
        {
            Debug.LogWarning("GameStateManager not found! Town state won't be loaded.");
            return;
        }

        // For each building plot, check if it has a building constructed
        foreach (BuildingPlot plot in buildingPlots)
        {
            string buildingName = plot.GetBuildingName();
            int buildingLevel = GameStateManager.Instance.GetBuildingLevel(buildingName);

            if (buildingLevel > 0)
            {
                // Building exists, construct it visually
                plot.ConstructBuilding(buildingLevel);
            }
        }

        Debug.Log("Town state loaded from GameStateManager");
    }

    /// <summary>
    /// Attempt to construct a building at the specified plot
    /// </summary>
    public bool TryConstructBuilding(BuildingPlot plot, BuildingData buildingData)
    {
        if (plot == null || buildingData == null)
        {
            Debug.LogWarning("Cannot construct: invalid plot or building data");
            return false;
        }

        // Check if player has enough resources
        if (!CanAffordBuilding(buildingData))
        {
            Debug.Log($"Cannot afford {buildingData.buildingName}");
            return false;
        }

        // Spend resources
        SpendResources(buildingData);

        // Construct the building
        plot.ConstructBuilding(1); // Level 1 for now

        // Save to GameStateManager
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SetBuildingLevel(buildingData.buildingName, 1);
        }

        Debug.Log($"Constructed {buildingData.buildingName}!");
        return true;
    }

    bool CanAffordBuilding(BuildingData buildingData)
    {
        if (GameStateManager.Instance == null) return false;

        int currentGold = GameStateManager.Instance.GetGold();
        return currentGold >= buildingData.goldCost;
    }

    void SpendResources(BuildingData buildingData)
    {
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.SpendGold(buildingData.goldCost);
        }
    }

    /// <summary>
    /// Get all building plots in the town
    /// </summary>
    public List<BuildingPlot> GetAllBuildingPlots()
    {
        return buildingPlots;
    }

    /// <summary>
    /// Get a specific building plot by name
    /// </summary>
    public BuildingPlot GetPlot(string plotName)
    {
        return buildingPlots.Find(p => p.GetBuildingName() == plotName);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}