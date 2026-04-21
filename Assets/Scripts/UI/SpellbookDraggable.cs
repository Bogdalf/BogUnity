using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Attach to each skill icon in the spellbook UI.
/// Handles drag-and-drop onto action bar slots.
/// </summary>
public class SpellbookDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SkillData SkillData { get; private set; }
    private Transform originalParent;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    [SerializeField] private Image iconImage;
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void Initialize(SkillData skillData)
    {
        SkillData = skillData;
        if (iconImage != null && skillData?.icon != null)
            iconImage.sprite = skillData.icon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        PersistentInputManager.Instance?.SetDragging(true);
        originalParent = transform.parent;

        // Move to root canvas so it renders on top of everything
        transform.SetParent(rootCanvas.transform, true);

        // Don't block raycasts while dragging so the drop target can receive them
        if (canvasGroup != null) canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / rootCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Return to spellbook regardless of whether drop succeeded
        transform.SetParent(originalParent, true);
        rectTransform.anchoredPosition = Vector2.zero;

        if (canvasGroup != null) canvasGroup.blocksRaycasts = true;
        PersistentInputManager.Instance?.SetDragging(false);
    }
}