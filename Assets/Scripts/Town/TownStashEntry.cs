using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI entry for a single material in the Town Stash.
/// Shows icon and quantity.
/// </summary>
public class TownStashEntry : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private TextMeshProUGUI nameText;

    private string materialName;
    private int quantity;

    public void Initialize(string matName, int qty)
    {
        materialName = matName;
        quantity = qty;

        // Try to find the material data to get the icon
        // For now, we'll just show the name and quantity
        // You can improve this later by maintaining a material database

        UpdateDisplay();
    }

    public void Initialize(CraftingMaterialData material, int qty)
    {
        if (material == null) return;

        materialName = material.itemName;
        quantity = qty;

        // Set icon
        if (iconImage != null && material.icon != null)
        {
            iconImage.sprite = material.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false; // Hide icon if no sprite
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (nameText != null)
        {
            nameText.text = materialName;
        }

        if (quantityText != null)
        {
            quantityText.text = $"x{quantity}";
        }

        // If icon wasn't set by Initialize, hide it
        if (iconImage != null && iconImage.sprite == null)
        {
            iconImage.enabled = false;
        }
    }
}