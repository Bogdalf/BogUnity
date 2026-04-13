using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryUI : MonoBehaviour, IUIPanel
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerRuneBook playerRuneBook;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform gridContainer;
    [SerializeField] private GameObject gridSlotPrefab;
    [SerializeField] private InventoryTrashZone trashZone;

    [Header("Equipment Display")]
    [SerializeField] private TextMeshProUGUI runeBookText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isInventoryOpen = false;
    private bool isMouseOverInventory = false;

    // Drag state
    private GameObject draggedItemVisual;
    private int dragFromX;
    private int dragFromY;
    private ItemData draggedItem;

    void Start()
    {
        FindPlayerReferences();
    }

    void Update()
    {
        if (inventory == null || playerRuneBook == null)
            FindPlayerReferences();

        if (Input.GetKeyDown(toggleKey) || Input.GetKeyDown(KeyCode.Tab))
            ToggleInventory();

        if (isInventoryOpen)
            UpdateEquipmentDisplay();
    }

    void FindPlayerReferences()
    {
        GameObject player = PersistentPlayer.Instance != null
            ? PersistentPlayer.Instance.gameObject
            : GameObject.FindGameObjectWithTag("Player");

        if (player == null) return;

        if (inventory == null)
            inventory = player.GetComponent<Inventory>();

        if (playerRuneBook == null)
            playerRuneBook = player.GetComponent<PlayerRuneBook>();

        if (inventory == null)
            Debug.LogWarning("InventoryUI: Could not find Inventory component!");

        if (playerRuneBook == null)
            Debug.LogWarning("InventoryUI: Could not find PlayerRuneBook component!");
    }

    void ToggleInventory()
    {
        if (isInventoryOpen)
            Close();
        else
            Open();
    }

    void Open()
    {
        isInventoryOpen = true;
        UIPanelManager.Instance?.OnPanelOpening(this, UIPanelManager.PanelRegion.Right);
        if (inventoryPanel != null) inventoryPanel.SetActive(true);
        RefreshGridDisplay();
    }

    public void Close()
    {
        isInventoryOpen = false;
        if (inventoryPanel != null) inventoryPanel.SetActive(false);
        UIPanelManager.Instance?.OnPanelClosed(this, UIPanelManager.PanelRegion.Right);
    }

    void RefreshGridDisplay()
    {
        if (inventory == null || gridContainer == null) return;

        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);

        int gridWidth = inventory.GetGridWidth();
        int gridHeight = inventory.GetGridHeight();

        for (int y = 0; y < gridHeight; y++)
            for (int x = 0; x < gridWidth; x++)
                CreateGridSlot(x, y);
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

    void UpdateEquipmentDisplay()
    {
        if (runeBookText != null && playerRuneBook != null)
        {
            RuneBookData book = playerRuneBook.GetEquippedBook();
            runeBookText.text = book != null ? book.itemName : "No Rune Book";
        }
    }

    // Called by InventorySlot when right-clicked
    public void OnSlotRightClicked(int x, int y, ItemData item)
    {
        if (item == null || inventory == null) return;

        if (item.itemType == ItemType.RuneBook)
        {
            RuneBookData book = item as RuneBookData;
            if (book != null && playerRuneBook != null)
            {
                playerRuneBook.EquipBook(book);
                Debug.Log("Equipped Rune Book: " + book.itemName);
                RefreshGridDisplay();
            }
        }
    }

    // ─── Drag and Drop ────────────────────────────────────────────────────────────

    public void OnBeginDrag(int fromX, int fromY, ItemData item, GameObject slotObject)
    {
        if (inventory == null) return;

        dragFromX = fromX;
        dragFromY = fromY;
        draggedItem = item;

        draggedItemVisual = new GameObject("DraggedItem");
        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
            draggedItemVisual.transform.SetParent(rootCanvas.transform, false);

        Image dragImage = draggedItemVisual.AddComponent<Image>();
        if (item?.icon != null)
            dragImage.sprite = item.icon;

        RectTransform rt = draggedItemVisual.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(50, 50);

        CanvasGroup cg = draggedItemVisual.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        // Reveal the trash zone while dragging
        trashZone?.ShowForDrag();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (draggedItemVisual != null)
            draggedItemVisual.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(int fromX, int fromY, PointerEventData eventData)
    {
        if (draggedItemVisual != null)
        {
            Destroy(draggedItemVisual);
            draggedItemVisual = null;
        }

        // Check trash zone BEFORE hiding it — HideAfterDrag resets IsMouseOver
        bool droppedOnTrash = trashZone != null && trashZone.IsMouseOver();

        // Always hide the trash zone when drag ends
        trashZone?.HideAfterDrag();

        if (inventory == null) { draggedItem = null; return; }

        if (droppedOnTrash)
        {
            inventory.RemoveItemAtPosition(fromX, fromY);
            Debug.Log("Item trashed!");
            RefreshGridDisplay();
            draggedItem = null;
            return;
        }

        // Find drop target slot
        GameObject dropTarget = eventData.pointerCurrentRaycast.gameObject;
        if (dropTarget != null)
        {
            InventorySlot targetSlot = dropTarget.GetComponent<InventorySlot>()
                ?? dropTarget.GetComponentInParent<InventorySlot>();

            if (targetSlot != null)
                inventory.MoveItem(fromX, fromY, targetSlot.GetGridX(), targetSlot.GetGridY());
        }

        RefreshGridDisplay();
        draggedItem = null;
    }

    /// <summary>Alias called by Inventory when it needs to trigger a UI refresh.</summary>
    public void RefreshDisplay() => RefreshGridDisplay();

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    public void SetMouseOverInventory(bool isOver) => isMouseOverInventory = isOver;
    public bool IsMouseOverInventory() => isMouseOverInventory;
    public bool IsInventoryOpen() => isInventoryOpen;
    public Inventory GetInventory() => inventory;
    public PlayerRuneBook GetPlayerRuneBook() => playerRuneBook;
}