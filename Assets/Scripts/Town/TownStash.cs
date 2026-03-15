using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Town Stash - Unlimited storage for crafting materials.
/// Materials are stored as quantities, not inventory slots.
/// Managed by GameStateManager for persistence.
/// </summary>
public class TownStash : MonoBehaviour
{
    public static TownStash Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject stashPanel;
    [SerializeField] private Transform stashContentParent; // Where material UI entries spawn
    [SerializeField] private GameObject stashEntryPrefab; // Prefab showing material icon + count

    private Dictionary<string, int> materialQuantities = new Dictionary<string, int>();
    private List<TownStashEntry> spawnedEntries = new List<TownStashEntry>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (stashPanel != null)
        {
            stashPanel.SetActive(false);
        }

        LoadStashData();
    }

    void Update()
    {
        // Toggle stash with B key (or whatever you prefer)
        if (Input.GetKeyDown(KeyCode.B))
        {
            ToggleStash();
        }
    }

    /// <summary>
    /// Add material to stash
    /// </summary>
    public void AddMaterial(CraftingMaterialData material, int amount)
    {
        if (material == null || amount <= 0) return;

        string materialName = material.itemName;

        if (materialQuantities.ContainsKey(materialName))
        {
            materialQuantities[materialName] += amount;
        }
        else
        {
            materialQuantities[materialName] = amount;
        }

        Debug.Log($"Added {amount}x {materialName} to stash. Total: {materialQuantities[materialName]}");

        // Save to GameStateManager
        SaveStashData();

        // Update UI if open
        if (stashPanel != null && stashPanel.activeSelf)
        {
            RefreshStashUI();
        }
    }

    /// <summary>
    /// Remove material from stash (returns true if successful)
    /// </summary>
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

        // Remove entry if quantity is 0
        if (materialQuantities[materialName] <= 0)
        {
            materialQuantities.Remove(materialName);
        }

        Debug.Log($"Removed {amount}x {materialName} from stash");

        SaveStashData();

        if (stashPanel != null && stashPanel.activeSelf)
        {
            RefreshStashUI();
        }

        return true;
    }

    /// <summary>
    /// Get quantity of a specific material
    /// </summary>
    public int GetMaterialQuantity(CraftingMaterialData material)
    {
        if (material == null) return 0;
        return materialQuantities.ContainsKey(material.itemName) ? materialQuantities[material.itemName] : 0;
    }

    /// <summary>
    /// Check if stash contains enough of a material
    /// </summary>
    public bool HasMaterial(CraftingMaterialData material, int amount)
    {
        return GetMaterialQuantity(material) >= amount;
    }

    public void ToggleStash()
    {
        if (stashPanel == null) return;

        bool isOpen = !stashPanel.activeSelf;
        stashPanel.SetActive(isOpen);

        if (isOpen)
        {
            RefreshStashUI();
        }
    }

    /// <summary>
    /// Button handler - Deposit all crafting materials from player inventory
    /// </summary>
    public void OnDepositAllButtonClicked()
    {
        if (PersistentPlayer.Instance == null) return;

        Inventory playerInventory = PersistentPlayer.Instance.GetComponent<Inventory>();
        if (playerInventory != null)
        {
            DepositAllMaterials(playerInventory);
        }
        else
        {
            Debug.LogWarning("Could not find player inventory!");
        }
    }

    /// <summary>
    /// Button handler - Close the stash
    /// </summary>
    public void OnCloseButtonClicked()
    {
        if (stashPanel != null)
        {
            stashPanel.SetActive(false);
        }
    }

    void RefreshStashUI()
    {
        Debug.Log($"RefreshStashUI called. Material count: {materialQuantities.Count}");

        // Clear existing entries
        foreach (TownStashEntry entry in spawnedEntries)
        {
            if (entry != null)
                Destroy(entry.gameObject);
        }
        spawnedEntries.Clear();

        if (stashContentParent == null)
        {
            Debug.LogWarning("Stash Content Parent is null!");
            return;
        }

        if (stashEntryPrefab == null)
        {
            Debug.LogWarning("Stash Entry Prefab is null!");
            return;
        }

        // Spawn new entries for each material
        foreach (KeyValuePair<string, int> material in materialQuantities)
        {
            Debug.Log($"Spawning entry for {material.Key}: {material.Value}");

            GameObject entryObj = Instantiate(stashEntryPrefab, stashContentParent);
            TownStashEntry entry = entryObj.GetComponent<TownStashEntry>();

            if (entry != null)
            {
                // Find the material data (you'll need to implement this lookup)
                // For now, just pass the name and quantity
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

    void SaveStashData()
    {
        if (GameStateManager.Instance == null) return;

        // Save all materials to GameStateManager
        foreach (KeyValuePair<string, int> material in materialQuantities)
        {
            GameStateManager.Instance.SetStashMaterial(material.Key, material.Value);
        }
    }

    void LoadStashData()
    {
        if (GameStateManager.Instance == null) return;

        materialQuantities = GameStateManager.Instance.GetAllStashMaterials();
        Debug.Log($"Loaded {materialQuantities.Count} material types from stash");
    }

    /// <summary>
    /// Transfer all crafting materials from player inventory to stash
    /// </summary>
    public void DepositAllMaterials(Inventory playerInventory)
    {
        if (playerInventory == null) return;

        int itemsDeposited = 0;

        // Loop through inventory and deposit crafting materials
        for (int y = 0; y < playerInventory.GetGridHeight(); y++)
        {
            for (int x = 0; x < playerInventory.GetGridWidth(); x++)
            {
                ItemData item = playerInventory.GetItemAtPosition(x, y);

                // Check if it's a crafting material
                if (item != null && item is CraftingMaterialData)
                {
                    CraftingMaterialData material = item as CraftingMaterialData;
                    int stackSize = playerInventory.GetStackSizeAtPosition(x, y);

                    // Add to stash (include stack size)
                    AddMaterial(material, stackSize);

                    // Remove from inventory
                    playerInventory.RemoveItemAtPosition(x, y);

                    itemsDeposited += stackSize;
                }
            }
        }

        Debug.Log($"Deposited {itemsDeposited} materials to stash");
    }
}