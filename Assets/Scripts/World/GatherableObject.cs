using UnityEngine;

public class GatherableObject : MonoBehaviour
{
    [Header("Gatherable Settings")]
    [SerializeField] private CraftingMaterialData materialToGive;
    [SerializeField] private int minDropAmount = 1;
    [SerializeField] private int maxDropAmount = 3;
    [SerializeField] private int maxHits = 3; // How many hits before it's depleted

    [Header("World Item Settings")]
    [SerializeField] private GameObject flyingResourcePrefab; // Prefab with FlyingResource script & sprite
    [SerializeField] private float dropForce = 2f; // Visual spread of flying resources

    [Header("Visual Feedback")]
    [SerializeField] private SpriteRenderer[] spriteRenderers; // Array for trunk + canopy
    [SerializeField] private Color depletedColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    [SerializeField] private Sprite depletedSprite; // Optional - replaces sprite when depleted (for main sprite)
    [SerializeField] private bool hideCanopyWhenDepleted = true; // Hide canopy, show stump

    private int currentHits = 0;
    private bool isDepleted = false;
    private Color[] originalColors;
    private Sprite[] originalSprites;

    void Start()
    {
        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            originalColors = new Color[spriteRenderers.Length];
            originalSprites = new Sprite[spriteRenderers.Length];

            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    originalColors[i] = spriteRenderers[i].color;
                    originalSprites[i] = spriteRenderers[i].sprite;
                }
            }
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

        // Add resources to player inventory immediately
        Inventory playerInventory = gatherer.GetComponent<Inventory>();
        if (playerInventory != null)
        {
            int itemsAdded = 0;

            for (int i = 0; i < amount; i++)
            {
                if (playerInventory.AddItemToFirstAvailableSlot(materialToGive))
                {
                    itemsAdded++;
                }
            }

            if (itemsAdded > 0)
            {
                Debug.Log("Gathered " + itemsAdded + "x " + materialToGive.itemName);

                // Spawn visual feedback for items added
                SpawnFlyingResources(itemsAdded, gatherer.transform);
            }

            // If inventory was full, remaining items just don't spawn visuals
            // Could optionally drop them as ground loot here
        }
    }

    void SpawnFlyingResources(int amount, Transform player)
    {
        if (flyingResourcePrefab == null || materialToGive == null) return;

        for (int i = 0; i < amount; i++)
        {
            // Spawn at tree position with slight vertical offset
            Vector3 spawnPos = transform.position + Vector3.up * 1f;

            GameObject resourceObj = Instantiate(flyingResourcePrefab, spawnPos, Quaternion.identity);

            // Initialize flying behavior
            FlyingResource flyingResource = resourceObj.GetComponent<FlyingResource>();
            if (flyingResource != null)
            {
                // Random burst direction (spread them out)
                Vector2 burstDir = Random.insideUnitCircle.normalized;

                flyingResource.Initialize(materialToGive.icon, player, burstDir);
            }
        }
    }

    void Deplete()
    {
        isDepleted = true;
        Debug.Log(materialToGive.itemName + " node depleted!");

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            // Trunk is usually index 0, Canopy is index 1
            // Change trunk to stump
            if (spriteRenderers.Length > 0 && spriteRenderers[0] != null)
            {
                if (depletedSprite != null)
                {
                    spriteRenderers[0].sprite = depletedSprite; // Trunk becomes stump
                }
                spriteRenderers[0].color = depletedColor;
            }

            // Hide canopy when depleted
            if (hideCanopyWhenDepleted && spriteRenderers.Length > 1 && spriteRenderers[1] != null)
            {
                spriteRenderers[1].gameObject.SetActive(false); // Hide canopy
            }
        }

        // Could destroy the object, respawn it later, or keep it depleted
        // For now, just mark it as depleted
        // Invoke("Respawn", 30f); // Respawn after 30 seconds (optional)
    }

    void Respawn()
    {
        currentHits = 0;
        isDepleted = false;

        if (spriteRenderers != null && spriteRenderers.Length > 0)
        {
            // Restore trunk
            if (spriteRenderers.Length > 0 && spriteRenderers[0] != null && originalSprites.Length > 0)
            {
                spriteRenderers[0].sprite = originalSprites[0];
                spriteRenderers[0].color = originalColors[0];
            }

            // Restore canopy
            if (spriteRenderers.Length > 1 && spriteRenderers[1] != null)
            {
                spriteRenderers[1].gameObject.SetActive(true); // Show canopy again
                if (originalSprites.Length > 1 && originalColors.Length > 1)
                {
                    spriteRenderers[1].sprite = originalSprites[1];
                    spriteRenderers[1].color = originalColors[1];
                }
            }
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