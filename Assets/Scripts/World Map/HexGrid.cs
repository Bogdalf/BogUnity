using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class HexMissionAssignment
{
    public Vector2Int coordinates; // Hex coordinates (q, r)
    public MissionData missionData;
    public Sprite customSprite; // Optional custom sprite for this hex
}

public class HexGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private float hexSize = 1f;
    [SerializeField] private bool generateOnStart = true;

    [Header("Hex Prefab")]
    [SerializeField] private GameObject hexNodePrefab;

    [Header("Grid Offset")]
    [SerializeField] private Vector2 gridOffset = Vector2.zero;

    [Header("Mission Assignments")]
    [SerializeField] private HexMissionAssignment[] missionAssignments;
    [SerializeField] private bool hideUnassignedHexes = true;

    private Dictionary<Vector2Int, HexNode> hexNodes = new Dictionary<Vector2Int, HexNode>();

    // Hex layout constants (flat-top hexagons)
    private readonly float sqrt3 = Mathf.Sqrt(3f);

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateGrid();
        }
    }

    public void GenerateGrid()
    {
        ClearGrid();

        for (int r = 0; r < gridHeight; r++)
        {
            for (int q = 0; q < gridWidth; q++)
            {
                CreateHexNode(q, r);
            }
        }

        // Assign missions after all hexes are created
        AssignMissions();
    }

    private void CreateHexNode(int q, int r)
    {
        Vector2 position = AxialToWorldPosition(q, r);

        GameObject hexObj = Instantiate(hexNodePrefab, transform);
        hexObj.transform.localPosition = new Vector3(position.x + gridOffset.x, position.y + gridOffset.y, 0);

        HexNode hexNode = hexObj.GetComponent<HexNode>();
        if (hexNode != null)
        {
            hexNode.Initialize(q, r);
            hexNodes[new Vector2Int(q, r)] = hexNode;
        }
    }

    // Convert axial coordinates to world position (flat-top hexagons)
    private Vector2 AxialToWorldPosition(int q, int r)
    {
        // For flat-top hexagons with proper offset
        // Only odd rows get offset by half a hex width
        float xOffset = (r % 2 == 1) ? sqrt3 * hexSize * 0.5f : 0f;
        float x = hexSize * sqrt3 * q + xOffset;
        float y = hexSize * 0.5f * r;
        return new Vector2(x, y);
    }

    // Get hex at axial coordinates
    public HexNode GetHexAt(int q, int r)
    {
        Vector2Int coords = new Vector2Int(q, r);
        return hexNodes.ContainsKey(coords) ? hexNodes[coords] : null;
    }

    public HexNode GetHexAt(Vector2Int coords)
    {
        return hexNodes.ContainsKey(coords) ? hexNodes[coords] : null;
    }

    // Get all adjacent hexes to a given hex
    public List<HexNode> GetAdjacentHexes(HexNode hex)
    {
        List<HexNode> adjacent = new List<HexNode>();

        // Axial direction vectors for flat-top hexagons
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // Right
            new Vector2Int(1, -1),  // Top-right
            new Vector2Int(0, -1),  // Top-left
            new Vector2Int(-1, 0),  // Left
            new Vector2Int(-1, 1),  // Bottom-left
            new Vector2Int(0, 1)    // Bottom-right
        };

        foreach (Vector2Int dir in directions)
        {
            HexNode neighbor = GetHexAt(hex.q + dir.x, hex.r + dir.y);
            if (neighbor != null)
            {
                adjacent.Add(neighbor);
            }
        }

        return adjacent;
    }

    // Get all hexes in the grid
    public List<HexNode> GetAllHexes()
    {
        return new List<HexNode>(hexNodes.Values);
    }

    // Assign missions to hexes based on mission assignments
    private void AssignMissions()
    {
        if (missionAssignments == null || missionAssignments.Length == 0)
            return;

        // Create a set of assigned coordinates for quick lookup
        HashSet<Vector2Int> assignedCoords = new HashSet<Vector2Int>();

        foreach (var assignment in missionAssignments)
        {
            HexNode hex = GetHexAt(assignment.coordinates);
            if (hex != null && assignment.missionData != null)
            {
                hex.missionData = assignment.missionData;
                assignedCoords.Add(assignment.coordinates);

                // Apply custom sprite if provided
                if (assignment.customSprite != null)
                {
                    hex.SetCustomSprite(assignment.customSprite);
                }
            }
        }

        // Hide unassigned hexes if option is enabled
        if (hideUnassignedHexes)
        {
            foreach (var kvp in hexNodes)
            {
                if (!assignedCoords.Contains(kvp.Key))
                {
                    kvp.Value.SetVisible(false);
                }
            }
        }
    }

    // Clear the entire grid
    public void ClearGrid()
    {
        foreach (var hex in hexNodes.Values)
        {
            if (hex != null)
            {
                Destroy(hex.gameObject);
            }
        }
        hexNodes.Clear();
    }

    // Resize grid (regenerates)
    public void ResizeGrid(int width, int height)
    {
        gridWidth = width;
        gridHeight = height;
        GenerateGrid();
    }

    // Set hex size (regenerates grid)
    public void SetHexSize(float size)
    {
        hexSize = size;
        GenerateGrid();
    }

    // Public getters
    public int GetGridWidth() => gridWidth;
    public int GetGridHeight() => gridHeight;
    public float GetHexSize() => hexSize;
}