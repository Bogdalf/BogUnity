using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Trash zone that appears only while the player is dragging an inventory item.
/// Drop an item here to delete it.
/// Attach to a UI GameObject inside the inventory panel.
/// </summary>
public class InventoryTrashZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Visual")]
    [SerializeField] private Image zoneImage;
    [SerializeField] private Color normalColor  = new Color(0.6f, 0.1f, 0.1f, 0.7f);
    [SerializeField] private Color hoverColor   = new Color(0.9f, 0.1f, 0.1f, 0.9f);

    private bool isMouseOver = false;

    void Awake()
    {
        if (zoneImage == null)
            zoneImage = GetComponent<Image>();

        // Hidden by default — only visible while dragging
        gameObject.SetActive(false);
    }

    // ─── Show / Hide (called by InventoryUI) ──────────────────────────────────────

    public void ShowForDrag()
    {
        gameObject.SetActive(true);
        isMouseOver = false;
        if (zoneImage != null)
            zoneImage.color = normalColor;
    }

    public void HideAfterDrag()
    {
        gameObject.SetActive(false);
        isMouseOver = false;
    }

    // ─── Hover ────────────────────────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        isMouseOver = true;
        if (zoneImage != null)
            zoneImage.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isMouseOver = false;
        if (zoneImage != null)
            zoneImage.color = normalColor;
    }

    public bool IsMouseOver() => isMouseOver;
}