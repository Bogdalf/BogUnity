using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages persistent game state across scenes.
/// Holds data that needs to survive scene transitions:
/// - Resources (gold, materials, influence)
/// - Town building states
/// - Mission completion
/// - Player progression flags
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [Header("Resources")]
    [SerializeField] private int gold = 0;
    [SerializeField] private int influence = 0;

    [Header("Crafting Materials")]
    private Dictionary<string, int> craftingMaterials = new Dictionary<string, int>();

    [Header("Town State")]
    // We'll expand this later when building system is implemented
    // For now, just tracking whether town exists
    private Dictionary<string, int> buildingLevels = new Dictionary<string, int>();

    [Header("Mission Progress")]
    private HashSet<string> completedMissions = new HashSet<string>();
    private Dictionary<string, int> regionInfluence = new Dictionary<string, int>();

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameStateManager initialized");
            InitializeDefaults();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializeDefaults()
    {
        // Set starting resources
        gold = 500;
        influence = 0;

        Debug.Log("Game state initialized with default values");
    }

    #region Resources

    public int GetGold()
    {
        return gold;
    }

    public bool SpendGold(int amount)
    {
        if (gold >= amount)
        {
            gold -= amount;
            Debug.Log($"Spent {amount} gold. Remaining: {gold}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough gold! Need {amount}, have {gold}");
            return false;
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"Gained {amount} gold. Total: {gold}");
    }

    public int GetInfluence()
    {
        return influence;
    }

    public void AddInfluence(int amount)
    {
        influence += amount;
        Debug.Log($"Gained {amount} influence. Total: {influence}");
    }

    #endregion

    #region Crafting Materials

    public void AddCraftingMaterial(string materialName, int amount)
    {
        if (craftingMaterials.ContainsKey(materialName))
        {
            craftingMaterials[materialName] += amount;
        }
        else
        {
            craftingMaterials[materialName] = amount;
        }

        Debug.Log($"Added {amount}x {materialName}. Total: {craftingMaterials[materialName]}");
    }

    public int GetCraftingMaterial(string materialName)
    {
        return craftingMaterials.ContainsKey(materialName) ? craftingMaterials[materialName] : 0;
    }

    public bool SpendCraftingMaterial(string materialName, int amount)
    {
        int current = GetCraftingMaterial(materialName);
        if (current >= amount)
        {
            craftingMaterials[materialName] -= amount;
            Debug.Log($"Spent {amount}x {materialName}. Remaining: {craftingMaterials[materialName]}");
            return true;
        }
        else
        {
            Debug.LogWarning($"Not enough {materialName}! Need {amount}, have {current}");
            return false;
        }
    }

    #endregion

    #region Town Buildings

    public void SetBuildingLevel(string buildingName, int level)
    {
        buildingLevels[buildingName] = level;
        Debug.Log($"{buildingName} set to level {level}");
    }

    public int GetBuildingLevel(string buildingName)
    {
        return buildingLevels.ContainsKey(buildingName) ? buildingLevels[buildingName] : 0;
    }

    public bool IsBuildingBuilt(string buildingName)
    {
        return GetBuildingLevel(buildingName) > 0;
    }

    #endregion

    #region Missions

    public void CompleteMission(string missionName)
    {
        if (!completedMissions.Contains(missionName))
        {
            completedMissions.Add(missionName);
            Debug.Log($"Mission completed: {missionName}");
        }
    }

    public bool IsMissionCompleted(string missionName)
    {
        return completedMissions.Contains(missionName);
    }

    public void AddRegionInfluence(string regionName, int amount)
    {
        if (regionInfluence.ContainsKey(regionName))
        {
            regionInfluence[regionName] += amount;
        }
        else
        {
            regionInfluence[regionName] = amount;
        }

        Debug.Log($"Region {regionName} influence: {regionInfluence[regionName]}");
    }

    public int GetRegionInfluence(string regionName)
    {
        return regionInfluence.ContainsKey(regionName) ? regionInfluence[regionName] : 0;
    }

    #endregion

    #region Save/Load (Placeholder for future implementation)

    public void SaveGame()
    {
        // TODO: Implement save system
        Debug.Log("Save game called (not yet implemented)");
    }

    public void LoadGame()
    {
        // TODO: Implement load system
        Debug.Log("Load game called (not yet implemented)");
    }

    #endregion
}