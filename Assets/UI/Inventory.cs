using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 4;

    [Header("Starting Items (for testing)")]
    [SerializeField] private List<ItemData> startingItems = new List<ItemData>();

    // 2D array to hold items
    private ItemData[,] inventoryGrid;

    // Track stack sizes for stackable items
    private int[,] stackSizes;

    private PlayerEquipment playerEquipment;

    void Start()
    {
        playerEquipment = GetComponent<PlayerEquipment>();

        // Initialize the grid
        inventoryGrid = new ItemData[gridWidth, gridHeight];
        stackSizes = new int[gridWidth, gridHeight];

        // Add starting items for testing
        foreach (ItemData item in startingItems)
        {
            if (item != null)
            {
                AddItemToFirstAvailableSlot(item);
            }
        }
    }

    // Add item to the first available slot (with stacking support)
    public bool AddItemToFirstAvailableSlot(ItemData item)
    {
        if (item == null) return false;

        // If item is stackable, first try to add to existing stacks
        if (item.isStackable)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    // Found a matching item
                    if (inventoryGrid[x, y] == item)
                    {
                        // Check if we can add to this stack
                        if (stackSizes[x, y] < item.maxStackSize)
                        {
                            stackSizes[x, y]++;
                            Debug.Log("Added to stack at (" + x + ", " + y + "). New stack size: " + stackSizes[x, y]);
                            return true;
                        }
                    }
                }
            }
        }

        // Either not stackable, or no existing stacks with room - find empty slot
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (inventoryGrid[x, y] == null)
                {
                    inventoryGrid[x, y] = item;
                    stackSizes[x, y] = 1; // Start with stack of 1
                    Debug.Log("Added " + item.itemName + " at (" + x + ", " + y + ")");
                    return true;
                }
            }
        }

        Debug.Log("Inventory full! Can't pick up " + item.itemName);
        return false;
    }

    // Add item to a specific grid position
    public bool AddItemAtPosition(ItemData item, int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight)
        {
            Debug.Log("Invalid grid position!");
            return false;
        }

        if (inventoryGrid[x, y] != null)
        {
            Debug.Log("Slot already occupied!");
            return false;
        }

        inventoryGrid[x, y] = item;
        stackSizes[x, y] = 1;
        return true;
    }

    // Remove item at a specific position
    public void RemoveItemAtPosition(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;

        ItemData item = inventoryGrid[x, y];
        if (item != null)
        {
            Debug.Log("Removed " + item.itemName + " from inventory");
            inventoryGrid[x, y] = null;
            stackSizes[x, y] = 0;
        }
    }

    // Get item at position
    public ItemData GetItemAtPosition(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
        return inventoryGrid[x, y];
    }

    // Move item from one slot to another
    public bool MoveItem(int fromX, int fromY, int toX, int toY)
    {
        ItemData item = GetItemAtPosition(fromX, fromY);
        if (item == null) return false;

        ItemData targetItem = GetItemAtPosition(toX, toY);

        // If target has the same stackable item, try to merge stacks
        if (targetItem == item && item.isStackable)
        {
            int fromStack = stackSizes[fromX, fromY];
            int toStack = stackSizes[toX, toY];
            int spaceInTarget = item.maxStackSize - toStack;

            if (spaceInTarget > 0)
            {
                int amountToMove = Mathf.Min(fromStack, spaceInTarget);

                stackSizes[toX, toY] += amountToMove;
                stackSizes[fromX, fromY] -= amountToMove;

                // If source stack is now empty, clear it
                if (stackSizes[fromX, fromY] <= 0)
                {
                    inventoryGrid[fromX, fromY] = null;
                    stackSizes[fromX, fromY] = 0;
                }

                Debug.Log("Merged stacks: moved " + amountToMove + " items");
                return true;
            }
            else
            {
                Debug.Log("Target stack is full!");
                return false;
            }
        }

        // Normal move to empty slot
        if (targetItem != null)
        {
            Debug.Log("Target slot occupied!");
            return false;
        }

        inventoryGrid[toX, toY] = item;
        stackSizes[toX, toY] = stackSizes[fromX, fromY];

        inventoryGrid[fromX, fromY] = null;
        stackSizes[fromX, fromY] = 0;
        return true;
    }

    // Equip weapon from inventory (only for weapons)
    public void EquipWeaponFromPosition(int x, int y)
    {
        ItemData item = GetItemAtPosition(x, y);

        if (item == null || item.itemType != ItemType.Weapon)
        {
            Debug.Log("Not a weapon!");
            return;
        }

        WeaponData weapon = item as WeaponData;
        if (weapon == null || playerEquipment == null) return;

        // Same equipping logic as before
        if (weapon.weaponType == WeaponType.TwoHanded)
        {
            playerEquipment.EquipMainHand(weapon);
            Debug.Log("Equipped " + weapon.itemName + " (Two-Handed)");
        }
        else if (weapon.weaponType == WeaponType.OneHanded)
        {
            playerEquipment.EquipMainHand(weapon);
            Debug.Log("Equipped " + weapon.itemName + " (Main Hand)");
        }
        else if (weapon.weaponType == WeaponType.Shield)
        {
            playerEquipment.EquipOffHand(weapon);
            Debug.Log("Equipped " + weapon.itemName + " (Off Hand)");
        }
    }

    // Getters
    public int GetGridWidth() { return gridWidth; }
    public int GetGridHeight() { return gridHeight; }

    public int GetStackSizeAtPosition(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return 0;
        return stackSizes[x, y];
    }
}