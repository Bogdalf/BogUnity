using UnityEngine;

public class GroundLoot : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int stackSize = 1;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 1.5f;
    [SerializeField] private KeyCode pickupKey = KeyCode.E;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject pickupPrompt; // Optional "Press E" UI

    private Transform playerTransform;
    private bool playerInRange = false;

    void Start()
    {
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // Set sprite from item data
        if (spriteRenderer != null && itemData != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }

        // Hide prompt initially
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(false);
        }
    }

    void Update()
    {
        if (playerTransform == null || itemData == null) return;

        // Check distance to player
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        playerInRange = distance <= pickupRange;

        // Show/hide pickup prompt
        if (pickupPrompt != null)
        {
            pickupPrompt.SetActive(playerInRange);
        }

        // Try to pickup
        if (playerInRange && Input.GetKeyDown(pickupKey))
        {
            TryPickup();
        }
    }

    void TryPickup()
    {
        if (playerTransform == null) return;

        Inventory playerInventory = playerTransform.GetComponent<Inventory>();
        if (playerInventory == null)
        {
            Debug.LogWarning("Player doesn't have Inventory component!");
            return;
        }

        // Try to add items to inventory
        int remaining = stackSize;

        for (int i = 0; i < stackSize; i++)
        {
            if (playerInventory.AddItemToFirstAvailableSlot(itemData))
            {
                remaining--;
            }
            else
            {
                // Inventory full, couldn't pick up all
                break;
            }
        }

        // Update or destroy this ground loot
        if (remaining <= 0)
        {
            // Picked up everything
            Debug.Log("Picked up " + stackSize + "x " + itemData.itemName);
            Destroy(gameObject);
        }
        else
        {
            // Only picked up some, reduce stack
            stackSize = remaining;
            Debug.Log("Inventory full! " + remaining + "x " + itemData.itemName + " remaining on ground");
        }
    }

    // Public method to initialize the ground loot (called when spawning)
    public void Initialize(ItemData item, int stack = 1)
    {
        itemData = item;
        stackSize = stack;

        // Update sprite if we already have a sprite renderer
        if (spriteRenderer != null && itemData != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Show pickup range in editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}