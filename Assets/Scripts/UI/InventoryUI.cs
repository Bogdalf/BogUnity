using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject gridSlotPrefab;
    [SerializeField] private InventoryTrashZone trashZone; // Optional trash zone

    [Header("Equipment Display")]
    [SerializeField] private TextMeshProUGUI mainHandText;
    [SerializeField] private TextMeshProUGUI offHandText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isInventoryOpen = false;

    // Track if mouse is over inventory
    private bool isMouseOverInventory = false;

    // Drag state
    private GameObject draggedItemVisual;
    private int dragFromX;
    private int dragFromY;
    private ItemData draggedItem;

    void Start()
    {
        // Find player references if not set
        FindPlayerReferences();
    }

    void Update()
    {
        // If references are null, try to find them again
        if (inventory == null || playerEquipment == null)
        {
            FindPlayerReferences();
        }

        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            UpdateEquipmentDisplay();
        }
    }

    void FindPlayerReferences()
    {
        // Try to find via PersistentPlayer first
        if (PersistentPlayer.Instance != null)
        {
            if (inventory == null)
            {
                inventory = PersistentPlayer.Instance.GetComponent<Inventory>();
                if (inventory != null)
                {
                    Debug.Log("InventoryUI: Found Inventory on PersistentPlayer");
                }
            }

            if (playerEquipment == null)
            {
                playerEquipment = PersistentPlayer.Instance.GetComponent<PlayerEquipment>();
                if (playerEquipment != null)
                {
                    Debug.Log("InventoryUI: Found PlayerEquipment on PersistentPlayer");
                }
            }
        }
        else
        {
            // Fallback - find by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                if (inventory == null)
                {
                    inventory = player.GetComponent<Inventory>();
                }

                if (playerEquipment == null)
                {
                    playerEquipment = player.GetComponent<PlayerEquipment>();
                }
            }
        }

        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: Could not find Inventory component!");
        }

        if (playerEquipment == null)
        {
            Debug.LogWarning("InventoryUI: Could not find PlayerEquipment component!");
        }
    }

    void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;

        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(isInventoryOpen);
        }

        if (isInventoryOpen)
        {
            RefreshGridDisplay();
        }
    }

    void RefreshGridDisplay()
    {
        if (inventory == null || gridContainer == null) return;

        // Clear existing slots
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        int gridWidth = inventory.GetGridWidth();
        int gridHeight = inventory.GetGridHeight();

        // Create a slot for every grid position
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                CreateGridSlot(x, y);
            }
        }
    }

    void CreateGridSlot(int x, int y)
    {
        if (gridSlotPrefab == null || gridContainer == null || inventory == null) return;

        GameObject slotObj = Instantiate(gridSlotPrefab, gridContainer);
        InventorySlot slot = slotObj.GetComponent<InventorySlot>();

        if (slot != null)
        {
            ItemData item = inventory.GetItemAtPosition(x, y);
            int stackSize = inventory.GetStackSizeAtPosition(x, y);
            slot.Initialize(x, y, item, stackSize, this);
        }
    }

    // Called by InventorySlot when right-clicked (EQUIP)
    public void OnSlotRightClicked(int x, int y, ItemData item)
    {
        if (item == null || inventory == null) return;

        Debug.Log("Right-clicked " + item.itemName + " at (" + x + ", " + y + ")");

        // Equip weapons when right-clicked
        if (item.itemType == ItemType.Weapon)
        {
            inventory.EquipWeaponFromPosition(x, y);
            RefreshGridDisplay();
        }
    }

    // Drag and Drop handlers
    public void OnBeginDrag(int fromX, int fromY, ItemData item, GameObject slotObject)
    {
        if (inventory == null) return;

        dragFromX = fromX;
        dragFromY = fromY;
        draggedItem = item;

        // Create a visual representation that follows the mouse
        draggedItemVisual = new GameObject("DraggedItem");
        draggedItemVisual.transform.SetParent(transform, false);

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            draggedItemVisual.transform.SetParent(canvas.transform, false);
        }

        Image image = draggedItemVisual.AddComponent<Image>();
        image.sprite = item.icon;
        image.raycastTarget = false;

        RectTransform rt = draggedItemVisual.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItemVisual != null)
        {
            draggedItemVisual.transform.position = Input.mousePosition;
        }
    }

    public void OnEndDrag(int fromX, int fromY, PointerEventData eventData)
    {
        if (inventory == null) return;

        // Destroy the visual
        if (draggedItemVisual != null)
        {
            Destroy(draggedItemVisual);
        }

        // Check if dropped on trash zone
        if (trashZone != null && trashZone.IsMouseOver())
        {
            inventory.RemoveItemAtPosition(fromX, fromY);
            Debug.Log("Item trashed!");
            RefreshGridDisplay();
            return;
        }

        // Find what slot we dropped on
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;

        if (dropTarget == null)
        {
            Debug.Log("Dropped outside inventory - cancelled");
            return;
        }

        // Check if we dropped on a valid inventory slot
        InventorySlot targetSlot = dropTarget.GetComponent<InventorySlot>();
        if (targetSlot == null)
        {
            // Might have dropped on a child (like the image), check parent
            targetSlot = dropTarget.GetComponentInParent<InventorySlot>();
        }

        if (targetSlot != null)
        {
            int toX = targetSlot.GetGridX();
            int toY = targetSlot.GetGridY();

            // Move the item
            if (inventory.MoveItem(fromX, fromY, toX, toY))
            {
                Debug.Log("Moved item from (" + fromX + ", " + fromY + ") to (" + toX + ", " + toY + ")");
            }
        }

        RefreshGridDisplay();
    }

    public void RefreshDisplay()
    {
        RefreshGridDisplay();
    }

    void UpdateEquipmentDisplay()
    {
        if (playerEquipment == null) return;

        if (mainHandText != null)
        {
            WeaponData mainHand = playerEquipment.GetMainHandWeapon();
            mainHandText.text = mainHand != null ? mainHand.itemName : "Empty";
        }

        if (offHandText != null)
        {
            WeaponData offHand = playerEquipment.GetOffHandWeapon();
            offHandText.text = offHand != null ? offHand.itemName : "Empty";
        }
    }

    // Track mouse over inventory
    public void SetMouseOverInventory(bool isOver)
    {
        isMouseOverInventory = isOver;
    }

    public bool IsMouseOverInventory()
    {
        return isMouseOverInventory;
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }

    // Getters
    public Inventory GetInventory() => inventory;
    public PlayerEquipment GetPlayerEquipment() => playerEquipment;
}