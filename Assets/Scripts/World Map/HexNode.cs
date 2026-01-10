using UnityEngine;
using UnityEngine.EventSystems;

public class HexNode : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Hex Coordinates")]
    public int q; // Axial coordinate Q
    public int r; // Axial coordinate R

    [Header("Mission Data")]
    public MissionData missionData;

    [Header("Visual Components")]
    [SerializeField] private SpriteRenderer borderRenderer;
    [SerializeField] private Color normalColor = new Color(1f, 1f, 1f, 0.3f);
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.6f);
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color currentLocationColor = new Color(0.2f, 1f, 0.2f, 0.8f);
    [SerializeField] private Color completedColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);

    public enum HexState
    {
        Unexplored,
        Available,
        CurrentLocation,
        Completed
    }

    private HexState currentState = HexState.Available;
    private bool isHovered = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (borderRenderer == null)
        {
            borderRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void Start()
    {
        UpdateVisuals();
    }

    public void Initialize(int qCoord, int rCoord, MissionData mission = null)
    {
        q = qCoord;
        r = rCoord;
        missionData = mission;
        gameObject.name = $"Hex_{q}_{r}";
    }

    public void SetState(HexState newState)
    {
        currentState = newState;
        UpdateVisuals();
    }

    public HexState GetState()
    {
        return currentState;
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (borderRenderer == null) return;

        // Priority: Selected > Current Location > Hovered > State-based
        if (isSelected)
        {
            borderRenderer.color = selectedColor;
        }
        else if (currentState == HexState.CurrentLocation)
        {
            borderRenderer.color = currentLocationColor;
        }
        else if (isHovered)
        {
            borderRenderer.color = hoverColor;
        }
        else
        {
            switch (currentState)
            {
                case HexState.Unexplored:
                    borderRenderer.color = new Color(1f, 1f, 1f, 0.1f);
                    break;
                case HexState.Available:
                    borderRenderer.color = normalColor;
                    break;
                case HexState.Completed:
                    borderRenderer.color = completedColor;
                    break;
            }
        }
    }

    // Pointer Events
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        UpdateVisuals();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        UpdateVisuals();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentState == HexState.Unexplored) return;

        WorldMapManager.Instance?.SelectHex(this);
    }

    // Hex coordinate utilities
    public Vector2Int GetAxialCoordinates()
    {
        return new Vector2Int(q, r);
    }

    public Vector3Int GetCubeCoordinates()
    {
        int x = q;
        int z = r;
        int y = -x - z;
        return new Vector3Int(x, y, z);
    }

    // Set custom sprite for this hex
    public void SetCustomSprite(Sprite sprite)
    {
        if (borderRenderer != null)
        {
            borderRenderer.sprite = sprite;
        }
    }

    // Set visibility of this hex
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    // Calculate distance to another hex
    public int DistanceTo(HexNode other)
    {
        Vector3Int a = GetCubeCoordinates();
        Vector3Int b = other.GetCubeCoordinates();
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z)) / 2;
    }
}