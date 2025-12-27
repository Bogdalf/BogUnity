using UnityEngine;

public enum ItemType
{
    Weapon,
    Consumable,
    CraftingMaterial,
    QuestItem
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public ItemType itemType;
    public Sprite icon; // For grid display

    [Header("Grid Size (future-proofing for 2x2 items, etc)")]
    public int gridWidth = 1;
    public int gridHeight = 1;

    [Header("Stack Settings")]
    public bool isStackable = false;
    public int maxStackSize = 1;
}