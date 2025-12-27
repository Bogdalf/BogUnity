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

    [Header("Equipment Display")]
    [SerializeField] private TextMeshProUGUI mainHandText;
    [SerializeField] private TextMeshProUGUI offHandText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isInventoryOpen = false;

    // Drag state
    private GameObject draggedItemVisual;
    private int dragFromX;
    private int dragFromY;
    private ItemData draggedItem;

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            UpdateEquipmentDisplay();
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
        if (gridSlotPrefab == null || gridContainer == null) return;

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
        if (item == null) return;

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
        dragFromX = fromX;
        dragFromY = fromY;
        draggedItem = item;

        // Create a visual representation that follows the mouse
        draggedItemVisual = new GameObject("DraggedItem");
        draggedItemVisual.transform.SetParent(gridContainer.parent); // Parent to inventory panel

        Image image = draggedItemVisual.AddComponent<Image>();
        if (item.icon != null)
        {
            image.sprite = item.icon;
        }
        else
        {
            image.color = new Color(0.5f, 0.5f, 0.5f, 0.8f); // Gray placeholder
        }

        RectTransform rt = draggedItemVisual.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80, 80); // Match slot size

        // Make it semi-transparent
        CanvasGroup canvasGroup = draggedItemVisual.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through to slots below

        Debug.Log("Started dragging " + item.itemName);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItemVisual != null)
        {
            // Follow the mouse
            draggedItemVisual.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(int fromX, int fromY, PointerEventData eventData)
    {
        // Clean up the visual
        if (draggedItemVisual != null)
        {
            Destroy(draggedItemVisual);
            draggedItemVisual = null;
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

            Debug.Log("Dropped on slot (" + toX + ", " + toY + ")");

            // Move the item
            if (inventory.MoveItem(fromX, fromY, toX, toY))
            {
                Debug.Log("Moved " + draggedItem.itemName + " from (" + fromX + ", " + fromY + ") to (" + toX + ", " + toY + ")");
                RefreshGridDisplay();
            }
            else
            {
                Debug.Log("Could not move item - target slot occupied or invalid");
            }
        }
        else
        {
            Debug.Log("Did not drop on a valid slot");
        }

        draggedItem = null;
    }

    void UpdateEquipmentDisplay()
    {
        if (playerEquipment == null) return;

        WeaponData mainHand = playerEquipment.GetMainHandWeapon();
        WeaponData offHand = playerEquipment.GetOffHandWeapon();

        if (mainHandText != null)
        {
            mainHandText.text = mainHand != null ?
                mainHand.itemName + "\n" + mainHand.minDamage + "-" + mainHand.maxDamage + " Dmg" :
                "Empty";
        }

        if (offHandText != null)
        {
            offHandText.text = offHand != null ?
                offHand.itemName + "\n" + offHand.minDamage + "-" + offHand.maxDamage + " Dmg" :
                "Empty";
        }
    }

    void OnEnable()
    {
        RefreshGridDisplay();
        UpdateEquipmentDisplay();
    }

    public bool IsInventoryOpen()
    {
        return isInventoryOpen;
    }
}