using UnityEngine;

/// <summary>
/// Draws a hexagon outline in Scene view to help with tilemap painting.
/// Attach to an empty GameObject.
/// </summary>
public class HexOutlineHelper : MonoBehaviour
{
    [Header("Hex Dimensions")]
    [SerializeField] private float hexRadius = 25f; // Distance from center to corner
    [SerializeField] private bool usePointyTop = true; // False for flat-top hex

    [Header("Visual")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField] private bool showCoordinates = true;

    void OnDrawGizmos()
    {
        Gizmos.color = outlineColor;

        Vector3[] corners = GetHexCorners();

        // Draw hex outline
        for (int i = 0; i < 6; i++)
        {
            Vector3 start = corners[i];
            Vector3 end = corners[(i + 1) % 6];
            Gizmos.DrawLine(start, end);
        }

        // Draw center cross
        Gizmos.DrawLine(transform.position + Vector3.left * 2, transform.position + Vector3.right * 2);
        Gizmos.DrawLine(transform.position + Vector3.down * 2, transform.position + Vector3.up * 2);

#if UNITY_EDITOR
        // Show corner coordinates
        if (showCoordinates)
        {
            UnityEditor.Handles.color = outlineColor;
            for (int i = 0; i < 6; i++)
            {
                Vector3 corner = corners[i];
                Vector3Int gridPos = Vector3Int.FloorToInt(corner);
                UnityEditor.Handles.Label(corner, $"({gridPos.x}, {gridPos.y})");
            }
        }
#endif
    }

    Vector3[] GetHexCorners()
    {
        Vector3[] corners = new Vector3[6];

        for (int i = 0; i < 6; i++)
        {
            float angleDeg = 60 * i;
            if (usePointyTop)
                angleDeg += 30; // Rotate 30° for pointy-top orientation

            float angleRad = Mathf.Deg2Rad * angleDeg;

            corners[i] = transform.position + new Vector3(
                hexRadius * Mathf.Cos(angleRad),
                hexRadius * Mathf.Sin(angleRad),
                0
            );
        }

        return corners;
    }

    /// <summary>
    /// Call this to print all corner coordinates to console
    /// </summary>
    [ContextMenu("Print Corner Coordinates")]
    void PrintCorners()
    {
        Vector3[] corners = GetHexCorners();
        Debug.Log("=== Hex Corner Coordinates ===");
        for (int i = 0; i < 6; i++)
        {
            Vector3Int gridPos = Vector3Int.FloorToInt(corners[i]);
            Debug.Log($"Corner {i}: ({gridPos.x}, {gridPos.y})");
        }
    }
}