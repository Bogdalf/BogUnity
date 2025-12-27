using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI itemNameText; // Optional
    [SerializeField] private TextMeshProUGUI stackSizeText; // For showing stack count

    private int gridX;
    private int gridY;
    private ItemData item;
    private InventoryUI inventoryUI;
    private int stackSize;

    // Drag state
    private bool isDragging = false;
    private Vector2 originalPosition;

    public void Initialize(int x, int y, ItemData itemData, int stack, InventoryUI ui)
    {
        gridX = x;
        gridY = y;
        item = itemData;
        stackSize = stack;
        inventoryUI = ui;

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (item != null)
        {
            // Slot has an item
            if (itemIcon != null)
            {
                itemIcon.enabled = true;
                itemIcon.sprite = item.icon != null ? item.icon : null;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.3f, 0.3f, 0.3f, 1f); // Darker for filled slots
            }

            if (itemNameText != null)
            {
                itemNameText.text = item.itemName;
            }

            // Show stack size if stackable and more than 1
            if (stackSizeText != null)
            {
                if (item.isStackable && stackSize > 1)
                {
                    stackSizeText.text = stackSize.ToString();
                    stackSizeText.enabled = true;
                }
                else
                {
                    stackSizeText.enabled = false;
                }
            }
        }
        else
        {
            // Empty slot
            if (itemIcon != null)
            {
                itemIcon.enabled = false;
            }

            if (backgroundImage != null)
            {
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Light gray for empty
            }

            if (itemNameText != null)
            {
                itemNameText.text = "";
            }

            if (stackSizeText != null)
            {
                stackSizeText.enabled = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI == null) return;

        // Only process clicks if we're not dragging
        if (isDragging) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Right click - equip weapon
            inventoryUI.OnSlotRightClicked(gridX, gridY, item);
        }
        // Left click does nothing for now
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Only allow dragging with left mouse button and if there's an item
        if (eventData.button != PointerEventData.InputButton.Left || item == null) return;

        isDragging = true;
        originalPosition = transform.position;

        if (inventoryUI != null)
        {
            inventoryUI.OnBeginDrag(gridX, gridY, item, this.gameObject);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || inventoryUI == null) return;

        inventoryUI.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        isDragging = false;

        if (inventoryUI != null)
        {
            inventoryUI.OnEndDrag(gridX, gridY, eventData);
        }
    }

    // Public getters for InventoryUI to identify which slot was dropped on
    public int GetGridX() { return gridX; }
    public int GetGridY() { return gridY; }
    public ItemData GetItem() { return item; }
}