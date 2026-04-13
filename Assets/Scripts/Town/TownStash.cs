using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Town Stash - Unlimited storage for crafting materials.
/// Materials are stored as quantities, not inventory slots.
/// Managed by GameStateManager for persistence.
/// </summary>
public class TownStash : MonoBehaviour, IUIPanel
{
    public static TownStash Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject stashPanel;
    [SerializeField] private Transform stashContentParent;
    [SerializeField] private GameObject stashEntryPrefab;

    private Dictionary<string, int> materialQuantities = new Dictionary<string, int>();
    private List<TownStashEntry> spawnedEntries = new List<TownStashEntry>();

    private bool isStashOpen = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        if (stashPanel != null)
            stashPanel.SetActive(false);

        LoadStashData();
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.B)) return;

        // Always allow B to close the stash if it is already open.
        // Otherwise respect input blocking (dialogue, cutscenes, etc.)
        if (isStashOpen)
        {
            ToggleStash();
            return;
        }

        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsPlayerInputBlocked())
            return;

        ToggleStash();
    }

    // ─── Open / Close ─────────────────────────────────────────────────────────────

    public void ToggleStash()
    {
        if (isStashOpen)
            Close();
        else
            OpenStash();
    }

    void OpenStash()
    {
        if (stashPanel == null) return;
        isStashOpen = true;
        UIPanelManager.Instance?.OnPanelOpening(this, UIPanelManager.PanelRegion.Center);
        stashPanel.SetActive(true);
        RefreshStashUI();
    }

    public void Close()
    {
        if (stashPanel == null) return;
        isStashOpen = false;
        stashPanel.SetActive(false);
        UIPanelManager.Instance?.OnPanelClosed(this, UIPanelManager.PanelRegion.Center);
    }

    /// <summary>Button handler — Close the stash.</summary>
    public void OnCloseButtonClicked() => Close();

    // ─── Deposit ──────────────────────────────────────────────────────────────────

    /// <summary>Button handler — Deposit all crafting materials from player inventory.</summary>
    public void OnDepositAllButtonClicked()
    {
        // Block during sequences or dialogue, but allow while stash is open
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsSequenceBlocked())
            return;

        if (PersistentPlayer.Instance == null) return;

        Inventory playerInventory = PersistentPlayer.Instance.GetComponent<Inventory>();
        if (playerInventory != null)
            DepositAllMaterials(playerInventory);
        else
            Debug.LogWarning("TownStash: Could not find player inventory!");
    }

    public void DepositAllMaterials(Inventory playerInventory)
    {
        if (playerInventory == null) return;

        int itemsDeposited = 0;

        for (int y = 0; y < playerInventory.GetGridHeight(); y++)
        {
            for (int x = 0; x < playerInventory.GetGridWidth(); x++)
            {
                ItemData item = playerInventory.GetItemAtPosition(x, y);

                if (item != null && item is CraftingMaterialData)
                {
                    CraftingMaterialData material = item as CraftingMaterialData;
                    int stackSize = playerInventory.GetStackSizeAtPosition(x, y);

                    AddMaterial(material, stackSize);
                    playerInventory.RemoveItemAtPosition(x, y);
                    itemsDeposited += stackSize;
                }
            }
        }

        // Refresh inventory UI to show removed items
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        inventoryUI?.RefreshDisplay();

        Debug.Log($"Deposited {itemsDeposited} materials to stash");
    }

    // ─── Add / Remove ─────────────────────────────────────────────────────────────

    public void AddMaterial(CraftingMaterialData material, int amount)
    {
        if (material == null || amount <= 0) return;

        string materialName = material.itemName;

        if (materialQuantities.ContainsKey(materialName))
            materialQuantities[materialName] += amount;
        else
            materialQuantities[materialName] = amount;

        Debug.Log($"Added {amount}x {materialName} to stash. Total: {materialQuantities[materialName]}");

        SaveStashData();

        if (stashPanel != null && stashPanel.activeSelf)
            RefreshStashUI();
    }

    public bool RemoveMaterial(CraftingMaterialData material, int amount)
    {
        if (material == null || amount <= 0) return false;

        string materialName = material.itemName;

        if (!materialQuantities.ContainsKey(materialName))
        {
            Debug.LogWarning($"Stash doesn't contain {materialName}");
            return false;
        }

        if (materialQuantities[materialName] < amount)
        {
            Debug.LogWarning($"Not enough {materialName} in stash. Need {amount}, have {materialQuantities[materialName]}");
            return false;
        }

        materialQuantities[materialName] -= amount;

        if (materialQuantities[materialName] <= 0)
            materialQuantities.Remove(materialName);

        Debug.Log($"Removed {amount}x {materialName} from stash");

        SaveStashData();

        if (stashPanel != null && stashPanel.activeSelf)
            RefreshStashUI();

        return true;
    }

    // ─── Queries ──────────────────────────────────────────────────────────────────

    public int GetMaterialQuantity(CraftingMaterialData material)
    {
        if (material == null) return 0;
        return materialQuantities.ContainsKey(material.itemName) ? materialQuantities[material.itemName] : 0;
    }

    public bool HasMaterial(CraftingMaterialData material, int amount)
    {
        return GetMaterialQuantity(material) >= amount;
    }

    public bool IsStashOpen() => isStashOpen;

    // ─── UI ───────────────────────────────────────────────────────────────────────

    void RefreshStashUI()
    {
        Debug.Log($"RefreshStashUI called. Material count: {materialQuantities.Count}");

        foreach (TownStashEntry entry in spawnedEntries)
            if (entry != null) Destroy(entry.gameObject);
        spawnedEntries.Clear();

        if (stashContentParent == null) { Debug.LogWarning("Stash Content Parent is null!"); return; }
        if (stashEntryPrefab == null)   { Debug.LogWarning("Stash Entry Prefab is null!");   return; }

        foreach (KeyValuePair<string, int> material in materialQuantities)
        {
            GameObject entryObj = Instantiate(stashEntryPrefab, stashContentParent);
            TownStashEntry entry = entryObj.GetComponent<TownStashEntry>();

            if (entry != null)
            {
                entry.Initialize(material.Key, material.Value);
                spawnedEntries.Add(entry);
            }
            else
            {
                Debug.LogWarning("Stash Entry Prefab doesn't have TownStashEntry component!");
            }
        }

        Debug.Log($"Spawned {spawnedEntries.Count} stash entries");
    }

    // ─── Persistence ──────────────────────────────────────────────────────────────

    void SaveStashData()
    {
        if (GameStateManager.Instance == null) return;

        foreach (KeyValuePair<string, int> material in materialQuantities)
            GameStateManager.Instance.SetStashMaterial(material.Key, material.Value);
    }

    void LoadStashData()
    {
        if (GameStateManager.Instance == null) return;

        materialQuantities = GameStateManager.Instance.GetAllStashMaterials();
        Debug.Log($"Loaded {materialQuantities.Count} material types from stash");
    }
}