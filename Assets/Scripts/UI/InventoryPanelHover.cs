using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryPanelHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventoryUI inventoryUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetMouseOverInventory(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryUI != null)
        {
            inventoryUI.SetMouseOverInventory(false);
        }
    }
}