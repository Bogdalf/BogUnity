using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a vertex color gradient to a UI Image component.
/// Attach alongside an Image component — no special shader needed.
///
/// Supports multiple gradient directions and blend modes.
/// The Image's Source Image should be a plain white sprite for best results,
/// or left as None (Unity's default white).
/// </summary>
[RequireComponent(typeof(Image))]
[ExecuteAlways] // Updates in edit mode so you can preview in the Inspector
public class UIGradient : MonoBehaviour, IMeshModifier
{
    public enum GradientDirection
    {
        TopToBottom,
        BottomToTop,
        LeftToRight,
        RightToLeft,
        TopLeftToBottomRight,
        TopRightToBottomLeft,
        BottomLeftToTopRight,
        BottomRightToTopLeft,
        Radial,         // Center bright, edges dark (or vice versa)
        DiamondCenter,  // Diamond shape from center
    }

    [Header("Gradient Colors")]
    [Tooltip("Color at the 'start' of the gradient (top, left, or center depending on direction).")]
    public Color colorStart = new Color(0.215f, 0.141f, 0.063f, 1f);   // Gold

    [Tooltip("Color at the 'end' of the gradient (bottom, right, or edge depending on direction).")]
    public Color colorEnd   = new Color(0f, 0f, 0f, 1f);       // Black

    [Header("Direction")]
    public GradientDirection direction = GradientDirection.TopToBottom;

    [Header("Options")]
    [Tooltip("Blend mode: Multiply tints the Image's existing color, Override replaces it.")]
    public bool multiplyWithImageColor = false;

    [Tooltip("Intensity of the gradient effect (0 = no effect, 1 = full gradient).")]
    [Range(0f, 1f)]
    public float intensity = 1f;

    private Image image;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void OnValidate()
    {
        // Force mesh rebuild when values change in the Inspector
        if (image != null)
            image.SetVerticesDirty();
    }

    void OnEnable()
    {
        if (image != null)
            image.SetVerticesDirty();
    }

    void OnDisable()
    {
        if (image != null)
            image.SetVerticesDirty();
    }

    // ─── IMeshModifier ────────────────────────────────────────────────────────────

    public void ModifyMesh(Mesh mesh) { }

    public void ModifyMesh(VertexHelper vh)
    {
        if (!isActiveAndEnabled) return;

        int vertexCount = vh.currentVertCount;
        if (vertexCount == 0) return;

        // Collect all vertices to find bounds
        UIVertex vertex = UIVertex.simpleVert;
        Vector2 min = new Vector2(float.MaxValue,  float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < vertexCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);
            if (vertex.position.x < min.x) min.x = vertex.position.x;
            if (vertex.position.y < min.y) min.y = vertex.position.y;
            if (vertex.position.x > max.x) max.x = vertex.position.x;
            if (vertex.position.y > max.y) max.y = vertex.position.y;
        }

        Vector2 size = max - min;

        // Apply gradient to each vertex
        for (int i = 0; i < vertexCount; i++)
        {
            vh.PopulateUIVertex(ref vertex, i);

            // Normalized position (0 to 1) within the rect
            float nx = size.x > 0 ? (vertex.position.x - min.x) / size.x : 0f;
            float ny = size.y > 0 ? (vertex.position.y - min.y) / size.y : 0f;

            float t = GetGradientT(nx, ny);

            // Apply intensity — lerp between no effect (white) and full gradient
            Color gradientColor = Color.Lerp(colorStart, colorEnd, t);
            Color finalColor    = Color.Lerp(Color.white, gradientColor, intensity);

            if (multiplyWithImageColor)
                vertex.color = MultiplyColors(vertex.color, finalColor);
            else
                vertex.color = ToColor32(finalColor);

            vh.SetUIVertex(vertex, i);
        }
    }

    // ─── Gradient T calculation ───────────────────────────────────────────────────

    float GetGradientT(float nx, float ny)
    {
        switch (direction)
        {
            case GradientDirection.TopToBottom:
                return 1f - ny;

            case GradientDirection.BottomToTop:
                return ny;

            case GradientDirection.LeftToRight:
                return nx;

            case GradientDirection.RightToLeft:
                return 1f - nx;

            case GradientDirection.TopLeftToBottomRight:
                return (nx + (1f - ny)) * 0.5f;

            case GradientDirection.TopRightToBottomLeft:
                return ((1f - nx) + (1f - ny)) * 0.5f;

            case GradientDirection.BottomLeftToTopRight:
                return (nx + ny) * 0.5f;

            case GradientDirection.BottomRightToTopLeft:
                return ((1f - nx) + ny) * 0.5f;

            case GradientDirection.Radial:
            {
                float dx = nx - 0.5f;
                float dy = ny - 0.5f;
                return Mathf.Clamp01(Mathf.Sqrt(dx * dx + dy * dy) * 2f);
            }

            case GradientDirection.DiamondCenter:
            {
                float dx = Mathf.Abs(nx - 0.5f);
                float dy = Mathf.Abs(ny - 0.5f);
                return Mathf.Clamp01((dx + dy) * 2f);
            }

            default:
                return 1f - ny;
        }
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    Color32 MultiplyColors(Color32 a, Color b)
    {
        return new Color32(
            (byte)(a.r * b.r),
            (byte)(a.g * b.g),
            (byte)(a.b * b.b),
            (byte)(a.a * b.a)
        );
    }

    Color32 ToColor32(Color c)
    {
        return new Color32(
            (byte)Mathf.Clamp(c.r * 255, 0, 255),
            (byte)Mathf.Clamp(c.g * 255, 0, 255),
            (byte)Mathf.Clamp(c.b * 255, 0, 255),
            (byte)Mathf.Clamp(c.a * 255, 0, 255)
        );
    }
}