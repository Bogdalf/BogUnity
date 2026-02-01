using UnityEngine;

/// <summary>
/// ScriptableObject that defines a building type.
/// Create instances via: Right Click > Create > Town > Building Data
/// </summary>
[CreateAssetMenu(fileName = "New Building", menuName = "Town/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("Building Info")]
    public string buildingName = "House";
    [TextArea(2, 4)]
    public string description = "A place for townsfolk to live.";

    [Header("Visual")]
    public Sprite buildingSprite;
    public Sprite plotSprite; // Empty plot appearance

    [Header("Costs")]
    public int goldCost = 100;
    public int woodCost = 0;
    public int stoneCost = 0;
    public int influenceCost = 0;

    [Header("Requirements")]
    public bool requiresUnlock = false;
    public string unlockRequirement = ""; // Description of what unlocks this

    [Header("Functionality")]
    public BuildingType buildingType = BuildingType.Residential;
    public int maxLevel = 3;

    [Header("Benefits")]
    [TextArea(2, 4)]
    public string benefits = "Increases town population.";

    public enum BuildingType
    {
        Residential,    // Houses, apartments
        Production,     // Workshop, forge, farm
        Military,       // Barracks, armory
        Commerce,       // Market, shop, tavern
        Special,        // Castle, temple, special buildings
        Utility         // Storage, warehouse
    }

    /// <summary>
    /// Get the cost for upgrading to a specific level
    /// </summary>
    public int GetGoldCostForLevel(int level)
    {
        // Simple scaling: each level costs more
        return goldCost * level;
    }

    public int GetWoodCostForLevel(int level)
    {
        return woodCost * level;
    }

    public int GetStoneCostForLevel(int level)
    {
        return stoneCost * level;
    }
}