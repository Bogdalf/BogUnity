using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attach to the Viewport GameObject inside TalentTreePanel.
/// Handles click-drag panning and scroll-wheel zooming of the TreeContent child.
///
/// Required hierarchy:
///   TalentTreePanel
///     └── Viewport  (this script + Image for Mask + Mask component)
///           └── TreeContent  (RectTransform — assign to treeContent field)
///                 ├── lineContainer
///                 └── nodeContainer
/// </summary>
public class TalentTreePanner : MonoBehaviour, IDragHandler, IScrollHandler, IPointerDownHandler
{
    [Header("Target")]
    [SerializeField] private RectTransform treeContent;

    [Header("Pan Settings")]
    [SerializeField] private float panSpeed = 1f;

    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float minZoom = 0.3f;
    [SerializeField] private float maxZoom = 1.5f;

    private Canvas rootCanvas;

    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
    }

    // ─── Reset ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets the tree to centered position at 1x scale.
    /// Called by TalentTreeUI when the panel opens.
    /// </summary>
    public void ResetView()
    {
        if (treeContent == null) return;
        treeContent.anchoredPosition = Vector2.zero;
        treeContent.localScale       = Vector3.one;
    }

    // ─── Pan ──────────────────────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (treeContent == null) return;

        float scaleFactor = rootCanvas != null ? rootCanvas.scaleFactor : 1f;
        treeContent.anchoredPosition += eventData.delta / scaleFactor * panSpeed;
    }

    // ─── Zoom ─────────────────────────────────────────────────────────────────────

    public void OnScroll(PointerEventData eventData)
    {
        if (treeContent == null) return;

        float scroll      = eventData.scrollDelta.y;
        float currentZoom = treeContent.localScale.x;
        float newZoom     = Mathf.Clamp(currentZoom + scroll * zoomSpeed, minZoom, maxZoom);

        // Zoom toward the mouse cursor position
        Vector2 mousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            treeContent, eventData.position,
            eventData.pressEventCamera, out mousePos);

        float zoomDelta = newZoom / currentZoom;
        treeContent.localScale       = Vector3.one * newZoom;
        treeContent.anchoredPosition -= mousePos * (zoomDelta - 1f) * newZoom;
    }
}