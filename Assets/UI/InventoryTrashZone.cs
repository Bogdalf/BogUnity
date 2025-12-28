using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryTrashZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private InventoryUI inventoryUI;

    private bool isMouseOver = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
    }

    public bool IsMouseOver()
    {
        return isMouseOver;
    }
}