using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private PlayerEquipment playerEquipment;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform weaponListContainer;
    [SerializeField] private GameObject weaponSlotPrefab;

    [Header("Equipment Slot UI")]
    [SerializeField] private TextMeshProUGUI mainHandText;
    [SerializeField] private TextMeshProUGUI offHandText;
    [SerializeField] private Button mainHandUnequipButton;
    [SerializeField] private Button offHandUnequipButton;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;

    private bool isInventoryOpen = false;

    void Update()
    {
        // Toggle inventory with I key
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }

        // Update equipment slots display if inventory is open
        if (isInventoryOpen)
        {
            UpdateEquipmentSlotsDisplay();
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
            RefreshInventoryDisplay();
            UpdateEquipmentSlotsDisplay();
        }
    }

    void RefreshInventoryDisplay()
    {
        if (inventory == null || weaponListContainer == null) return;

        // Clear existing slots
        foreach (Transform child in weaponListContainer)
        {
            Destroy(child.gameObject);
        }

        // Create a slot for each weapon
        foreach (WeaponData weapon in inventory.GetWeapons())
        {
            CreateWeaponSlot(weapon);
        }
    }

    void CreateWeaponSlot(WeaponData weapon)
    {
        if (weaponSlotPrefab == null || weaponListContainer == null) return;

        GameObject slot = Instantiate(weaponSlotPrefab, weaponListContainer);

        // Set weapon name
        TextMeshProUGUI nameText = slot.transform.Find("WeaponName")?.GetComponent<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = weapon.weaponName;
        }

        // Set weapon stats
        TextMeshProUGUI statsText = slot.transform.Find("WeaponStats")?.GetComponent<TextMeshProUGUI>();
        if (statsText != null)
        {
            string typeText = weapon.weaponType.ToString();
            string damageText = weapon.minDamage + "-" + weapon.maxDamage;
            statsText.text = typeText + " | Dmg: " + damageText;
        }

        // Check if weapon is currently equipped
        bool isEquipped = IsWeaponEquipped(weapon);

        // Set up equip button
        Button equipButton = slot.transform.Find("EquipButton")?.GetComponent<Button>();
        if (equipButton != null)
        {
            equipButton.onClick.AddListener(() => OnEquipWeapon(weapon));

            // Change button text if equipped
            TextMeshProUGUI buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null && isEquipped)
            {
                buttonText.text = "EQUIPPED";
                equipButton.interactable = false; // Disable if already equipped
            }
        }

        // Set up drop button
        Button dropButton = slot.transform.Find("DropButton")?.GetComponent<Button>();
        if (dropButton != null)
        {
            dropButton.onClick.AddListener(() => OnDropWeapon(weapon));

            // Can't drop equipped weapons
            if (isEquipped)
            {
                dropButton.interactable = false;
            }
        }

        // Highlight if equipped
        if (isEquipped)
        {
            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.color = new Color(0.3f, 0.8f, 0.3f, 0.5f); // Green tint
            }
        }
    }

    bool IsWeaponEquipped(WeaponData weapon)
    {
        if (playerEquipment == null) return false;
        return playerEquipment.GetMainHandWeapon() == weapon ||
               playerEquipment.GetOffHandWeapon() == weapon;
    }

    void OnEquipWeapon(WeaponData weapon)
    {
        if (inventory != null)
        {
            inventory.EquipWeaponFromInventory(weapon);
            RefreshInventoryDisplay(); // Refresh to update button states
        }
    }

    void OnDropWeapon(WeaponData weapon)
    {
        // Don't drop equipped weapons
        if (IsWeaponEquipped(weapon))
        {
            Debug.Log("Can't drop equipped weapon! Unequip it first.");
            return;
        }

        if (inventory != null)
        {
            inventory.RemoveWeapon(weapon);
            RefreshInventoryDisplay();
            Debug.Log("Dropped: " + weapon.weaponName);
        }
    }

    void UpdateEquipmentSlotsDisplay()
    {
        if (playerEquipment == null) return;

        WeaponData mainHand = playerEquipment.GetMainHandWeapon();
        WeaponData offHand = playerEquipment.GetOffHandWeapon();

        // Update main hand display
        if (mainHandText != null)
        {
            if (mainHand != null)
            {
                mainHandText.text = mainHand.weaponName + "\n" +
                                   mainHand.minDamage + "-" + mainHand.maxDamage + " Dmg";
            }
            else
            {
                mainHandText.text = "Empty";
            }
        }

        // Update off hand display
        if (offHandText != null)
        {
            if (offHand != null)
            {
                offHandText.text = offHand.weaponName + "\n" +
                                  offHand.minDamage + "-" + offHand.maxDamage + " Dmg";
            }
            else
            {
                offHandText.text = "Empty";
            }
        }

        // Update unequip buttons
        if (mainHandUnequipButton != null)
        {
            mainHandUnequipButton.interactable = (mainHand != null);
            mainHandUnequipButton.onClick.RemoveAllListeners();
            mainHandUnequipButton.onClick.AddListener(() => OnUnequipMainHand());
        }

        if (offHandUnequipButton != null)
        {
            offHandUnequipButton.interactable = (offHand != null);
            offHandUnequipButton.onClick.RemoveAllListeners();
            offHandUnequipButton.onClick.AddListener(() => OnUnequipOffHand());
        }
    }

    void OnUnequipMainHand()
    {
        if (playerEquipment != null)
        {
            playerEquipment.UnequipMainHand();
            RefreshInventoryDisplay();
        }
    }

    void OnUnequipOffHand()
    {
        if (playerEquipment != null)
        {
            playerEquipment.UnequipOffHand();
            RefreshInventoryDisplay();
        }
    }

    // Call this when inventory opens to ensure it's up to date
    void OnEnable()
    {
        RefreshInventoryDisplay();
        UpdateEquipmentSlotsDisplay();
    }
}