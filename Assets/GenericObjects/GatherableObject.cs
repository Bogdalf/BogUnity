using UnityEngine;

public class GatherableObject : MonoBehaviour
{
    [Header("Gatherable Settings")]
    [SerializeField] private CraftingMaterialData materialToGive;
    [SerializeField] private int minDropAmount = 1;
    [SerializeField] private int maxDropAmount = 3;
    [SerializeField] private int maxHits = 3; // How many hits before it's depleted

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color depletedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

    private int currentHits = 0;
    private bool isDepleted = false;
    private Color originalColor;

    void Start()
    {
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // Called when the player hits this object with their gathering tool
    public void OnGathered(GameObject gatherer)
    {
        if (isDepleted)
        {
            Debug.Log("This resource is depleted!");
            return;
        }

        currentHits++;
        Debug.Log("Gathered! Hits: " + currentHits + "/" + maxHits);

        // Give the player resources
        GiveResources(gatherer);

        // Check if depleted
        if (currentHits >= maxHits)
        {
            Deplete();
        }
        else
        {
            // Visual feedback - shake or change color slightly
            UpdateVisuals();
        }
    }

    void GiveResources(GameObject gatherer)
    {
        if (materialToGive == null)
        {
            Debug.LogWarning("No material assigned to gatherable object!");
            return;
        }

        // Determine how much to give
        int amount = Random.Range(minDropAmount, maxDropAmount + 1);

        // Try to add to player's inventory
        Inventory playerInventory = gatherer.GetComponent<Inventory>();
        if (playerInventory != null)
        {
            // For now, just add to first available slot
            // Later we'll handle stacking properly
            for (int i = 0; i < amount; i++)
            {
                playerInventory.AddItemToFirstAvailableSlot(materialToGive);
            }

            Debug.Log("Gathered " + amount + "x " + materialToGive.itemName);
        }
    }

    void UpdateVisuals()
    {
        if (spriteRenderer == null) return;

        // Fade the sprite a bit with each hit
        float alphaReduction = 1f - ((float)currentHits / maxHits);
        Color newColor = originalColor;
        newColor.a = Mathf.Max(0.3f, alphaReduction);
        spriteRenderer.color = newColor;
    }

    void Deplete()
    {
        isDepleted = true;
        Debug.Log(materialToGive.itemName + " node depleted!");

        if (spriteRenderer != null)
        {
            spriteRenderer.color = depletedColor;
        }

        // Could destroy the object, respawn it later, or keep it depleted
        // For now, just mark it as depleted
        // Invoke("Respawn", 30f); // Respawn after 30 seconds (optional)
    }

    void Respawn()
    {
        currentHits = 0;
        isDepleted = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log("Resource node respawned!");
    }

    // Helper to check if this can be gathered
    public bool CanBeGathered()
    {
        return !isDepleted;
    }

    public CraftingMaterialData GetMaterial()
    {
        return materialToGive;
    }
}