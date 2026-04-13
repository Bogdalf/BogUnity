using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 5;
    [SerializeField] private int gridHeight = 4;

    [Header("Starting Items (for testing)")]
    [SerializeField] private List<ItemData> startingItems = new List<ItemData>();

    private ItemData[,] inventoryGrid;
    private int[,] stackSizes;

    private PlayerRuneBook playerRuneBook;
    private InventoryUI inventoryUI;

    void Start()
    {
        playerRuneBook = GetComponent<PlayerRuneBook>();
        inventoryUI = FindFirstObjectByType<InventoryUI>();

        inventoryGrid = new ItemData[gridWidth, gridHeight];
        stackSizes = new int[gridWidth, gridHeight];

        foreach (ItemData item in startingItems)
        {
            if (item != null)
                AddItemToFirstAvailableSlot(item);
        }
    }

    // ─── Add / Remove ─────────────────────────────────────────────────────────────

    public bool AddItemToFirstAvailableSlot(ItemData item)
    {
        if (item == null) return false;

        // Try to stack first
        if (item.isStackable)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (inventoryGrid[x, y] == item && stackSizes[x, y] < item.maxStackSize)
                    {
                        stackSizes[x, y]++;
                        Debug.Log("Added to stack at (" + x + ", " + y + "). New size: " + stackSizes[x, y]);
                        inventoryUI?.RefreshDisplay();
                        return true;
                    }
                }
            }
        }

        // Find empty slot
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (inventoryGrid[x, y] == null)
                {
                    inventoryGrid[x, y] = item;
                    stackSizes[x, y] = 1;
                    Debug.Log("Added " + item.itemName + " at (" + x + ", " + y + ")");
                    inventoryUI?.RefreshDisplay();
                    return true;
                }
            }
        }

        Debug.Log("Inventory full! Can't pick up " + item.itemName);
        return false;
    }

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

    // ─── Move / Equip ─────────────────────────────────────────────────────────────

    public bool MoveItem(int fromX, int fromY, int toX, int toY)
    {
        ItemData item = GetItemAtPosition(fromX, fromY);
        if (item == null) return false;

        ItemData targetItem = GetItemAtPosition(toX, toY);

        // Merge stacks
        if (targetItem == item && item.isStackable)
        {
            int spaceInTarget = item.maxStackSize - stackSizes[toX, toY];
            if (spaceInTarget > 0)
            {
                int amountToMove = Mathf.Min(stackSizes[fromX, fromY], spaceInTarget);
                stackSizes[toX, toY] += amountToMove;
                stackSizes[fromX, fromY] -= amountToMove;

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

    /// <summary>
    /// Equips a Rune Book from the given inventory position.
    /// Called by InventoryUI on right-click.
    /// </summary>
    public void EquipRuneBookFromPosition(int x, int y)
    {
        ItemData item = GetItemAtPosition(x, y);

        if (item == null || item.itemType != ItemType.RuneBook)
        {
            Debug.Log("Not a Rune Book!");
            return;
        }

        RuneBookData book = item as RuneBookData;
        if (book == null || playerRuneBook == null) return;

        playerRuneBook.EquipBook(book);
        Debug.Log("Equipped Rune Book: " + book.itemName);
    }

    // ─── Getters ──────────────────────────────────────────────────────────────────

    public ItemData GetItemAtPosition(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return null;
        return inventoryGrid[x, y];
    }

    public int GetGridWidth()  => gridWidth;
    public int GetGridHeight() => gridHeight;

    public int GetStackSizeAtPosition(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return 0;
        return stackSizes[x, y];
    }
}