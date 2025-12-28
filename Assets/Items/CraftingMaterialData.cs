using UnityEngine;

[CreateAssetMenu(fileName = "NewCraftingMaterial", menuName = "Inventory/Crafting Material")]
public class CraftingMaterialData : ItemData
{
    [Header("Crafting Material Info")]
    public string description;
    public CraftingMaterialType materialType;

    void OnValidate()
    {
        // Auto-set the item type
        itemType = ItemType.CraftingMaterial;

        // Crafting materials ARE stackable
        isStackable = true;
        if (maxStackSize == 1)
        {
            maxStackSize = 99;
        }
    }
}

public enum CraftingMaterialType
{
    Wood,
    Stone,
    Iron,
    Herb,
    Leather,
    Gem
}