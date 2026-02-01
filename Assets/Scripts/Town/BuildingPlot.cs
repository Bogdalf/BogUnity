using UnityEngine;

/// <summary>
/// Marks a location where a building can be constructed in the town.
/// Place these at designated building spots in the Town scene.
/// </summary>
public class BuildingPlot : MonoBehaviour
{
    [Header("Plot Settings")]
    [SerializeField] private string buildingName = "House"; // Unique identifier
    [SerializeField] private BuildingData allowedBuilding; // What can be built here

    [Header("Visual")]
    [SerializeField] private SpriteRenderer plotVisual; // Shows empty plot
    [SerializeField] private GameObject constructedBuildingVisual; // The actual building
    [SerializeField] private bool showPlotWhenEmpty = true;

    [Header("Interaction")]
    [SerializeField] private bool canInteract = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isConstructed = false;
    private int currentLevel = 0;
    private bool playerInRange = false;

    void Start()
    {
        UpdateVisuals();
    }

    void Update()
    {
        if (playerInRange && canInteract && !isConstructed)
        {
            if (Input.GetKeyDown(interactKey))
            {
                TryConstruct();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            if (!isConstructed)
            {
                ShowConstructionPrompt();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            HideConstructionPrompt();
        }
    }

    /// <summary>
    /// Attempt to construct the building at this plot
    /// </summary>
    void TryConstruct()
    {
        if (isConstructed)
        {
            Debug.Log("Building already constructed here");
            return;
        }

        if (allowedBuilding == null)
        {
            Debug.LogWarning("No building data assigned to this plot!");
            return;
        }

        // Try to construct via TownManager
        if (TownManager.Instance != null)
        {
            bool success = TownManager.Instance.TryConstructBuilding(this, allowedBuilding);
            // Visual update happens in ConstructBuilding() called by TownManager
        }
    }

    /// <summary>
    /// Construct the building (called by TownManager after resource check)
    /// </summary>
    public void ConstructBuilding(int level)
    {
        isConstructed = true;
        currentLevel = level;
        UpdateVisuals();

        Debug.Log($"{buildingName} constructed at level {level}");
    }

    void UpdateVisuals()
    {
        // Show/hide empty plot visual
        if (plotVisual != null)
        {
            plotVisual.gameObject.SetActive(!isConstructed && showPlotWhenEmpty);
        }

        // Show/hide constructed building
        if (constructedBuildingVisual != null)
        {
            constructedBuildingVisual.SetActive(isConstructed);
        }
    }

    void ShowConstructionPrompt()
    {
        if (allowedBuilding != null)
        {
            Debug.Log($"Press {interactKey} to build {allowedBuilding.buildingName} (Cost: {allowedBuilding.goldCost} gold)");
            // TODO: Show UI prompt
        }
    }

    void HideConstructionPrompt()
    {
        // TODO: Hide UI prompt
    }

    // Getters
    public string GetBuildingName() => buildingName;
    public bool IsConstructed() => isConstructed;
    public int GetLevel() => currentLevel;
    public BuildingData GetAllowedBuilding() => allowedBuilding;

    // Debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = isConstructed ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2f);

        // Draw label
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.5f, buildingName);
#endif
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 2.5f);
    }
}