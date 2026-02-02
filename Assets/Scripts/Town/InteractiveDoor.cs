using UnityEngine;

/// <summary>
/// Simple teleport door for building interiors.
/// Press E near door to teleport to destination point.
/// </summary>
public class InteractiveDoor : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportDestination; // Where to teleport player
    [SerializeField] private string doorName = "Door"; // For debug/prompt

    private bool playerInRange = false;

    void Update()
    {
        if (!playerInRange) return;

        // Press E to teleport
        if (Input.GetKeyDown(KeyCode.E))
        {
            TeleportPlayer();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Press E to use {doorName}");
            // TODO: Show UI prompt
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

    void TeleportPlayer()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning("InteractiveDoor: No teleport destination set!");
            return;
        }

        // Get persistent player
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetPosition(teleportDestination.position);

            // Snap camera to avoid smooth follow across teleport
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Vector3 cameraPos = teleportDestination.position;
                cameraPos.z = mainCamera.transform.position.z; // Keep camera's Z
                mainCamera.transform.position = cameraPos;
            }

            Debug.Log($"Teleported to {doorName} destination");
        }
        else
        {
            Debug.LogWarning("InteractiveDoor: Could not find PersistentPlayer!");
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Draw trigger area
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)boxCollider.offset, boxCollider.size);
        }

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Gizmos.DrawWireSphere(transform.position + (Vector3)circleCollider.offset, circleCollider.radius);
        }

        // Draw line to destination
        if (teleportDestination != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, teleportDestination.position);
            Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
        }
    }
}