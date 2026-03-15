using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Shows grid coordinates in Scene view for easy measurement.
/// Attach to Grid GameObject.
/// Only runs in editor, not in builds.
/// </summary>
public class GridCoordinateOverlay : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Display Settings")]
    [SerializeField] private bool showCoordinates = true;
    [SerializeField] private bool showEveryTile = false; // False = show every 5th
    [SerializeField] private int showInterval = 5; // Show every Nth tile
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 12;

    [Header("Grid Bounds")]
    [SerializeField] private Vector2Int minBounds = new Vector2Int(-20, -20);
    [SerializeField] private Vector2Int maxBounds = new Vector2Int(20, 20);

    private Grid grid;
    private GUIStyle labelStyle;

    void OnDrawGizmos()
    {
        if (!showCoordinates) return;

        if (grid == null)
        {
            grid = GetComponent<Grid>();
            if (grid == null) return;
        }

        // Setup label style
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle();
            labelStyle.fontSize = fontSize;
            labelStyle.normal.textColor = textColor;
            labelStyle.alignment = TextAnchor.MiddleCenter;
        }

        // Draw coordinate labels
        for (int x = minBounds.x; x <= maxBounds.x; x++)
        {
            for (int y = minBounds.y; y <= maxBounds.y; y++)
            {
                // Skip if not showing every tile
                if (!showEveryTile)
                {
                    if (x % showInterval != 0 || y % showInterval != 0)
                        continue;
                }

                // Get world position of this grid cell
                Vector3 cellCenter = grid.GetCellCenterWorld(new Vector3Int(x, y, 0));

                // Draw label in Scene view
                Handles.Label(cellCenter, $"{x},{y}", labelStyle);
            }
        }
    }
#endif
}