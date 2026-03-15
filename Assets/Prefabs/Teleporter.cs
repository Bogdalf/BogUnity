using UnityEngine;

/// <summary>
/// Automatic teleport when player walks over this trigger.
/// No button press required - instant teleport on collision.
/// Good for portals, stairs, zone transitions, etc.
/// </summary>
public class TeleportTrigger : MonoBehaviour
{
    [Header("Teleport Settings")]
    [SerializeField] private Transform teleportDestination; // Where to teleport player
    [SerializeField] private string triggerName = "Teleport"; // For debug

    [Header("Options")]
    [SerializeField] private bool snapCamera = true; // Instantly move camera too
    [SerializeField] private float cooldown = 1f; // Prevent rapid re-teleporting
    [SerializeField] private bool requireDirection = false; // Only teleport if moving from specific direction
    [SerializeField] private Vector2 requiredDirection = Vector2.up; // Direction player must be moving (e.g., up for stairs)

    [Header("Confirmation Dialogue (Optional)")]
    [SerializeField] private bool requireConfirmation = false; // Show Yes/No dialogue before teleporting
    [SerializeField] private string confirmationMessage = "Leave this area?"; // Message to show
    [SerializeField] private string confirmationTitle = "Teleport"; // Dialogue box title

    private float lastTeleportTime = -999f;
    private bool dialogueActive = false;
    private bool playerInTrigger = false;
    private int currentSelection = 0; // 0 = Yes, 1 = No

    void Update()
    {
        if (!playerInTrigger || !dialogueActive) return;

        // Handle Yes/No selection
        HandleDialogueInput();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Only teleport player
        if (!collision.CompareTag("Player")) return;

        // Check cooldown (prevent teleport loops)
        if (Time.time < lastTeleportTime + cooldown) return;

        // Optional: Check if player is moving in required direction
        if (requireDirection)
        {
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 playerVelocity = playerRb.linearVelocity.normalized;
                float dot = Vector2.Dot(playerVelocity, requiredDirection.normalized);

                // Player must be moving roughly in the required direction (dot > 0.5 means within ~60 degrees)
                if (dot < 0.5f)
                {
                    return; // Not moving in required direction, don't teleport
                }
            }
        }

        playerInTrigger = true;

        // Show confirmation dialogue or teleport immediately
        if (requireConfirmation)
        {
            ShowConfirmationDialogue();
        }
        else
        {
            TeleportPlayer();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInTrigger = false;
            dialogueActive = false;

            // Hide dialogue
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideDialogue();
            }
        }
    }

    void ShowConfirmationDialogue()
    {
        dialogueActive = true;
        currentSelection = 0; // Default to "Yes"
        UpdateDialogueDisplay();
    }

    void HandleDialogueInput()
    {
        // Navigate between choices
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection = 0; // Select "Yes"
            UpdateDialogueDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = 1; // Select "No"
            UpdateDialogueDisplay();
        }

        // Confirm choice
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentSelection == 0)
            {
                // Yes - Teleport
                ConfirmTeleport();
            }
            else
            {
                // No - Cancel
                CancelTeleport();
            }
        }
    }

    void UpdateDialogueDisplay()
    {
        if (DialogueUI.Instance == null) return;

        string yesOption = currentSelection == 0 ? "> Yes" : "  Yes";
        string noOption = currentSelection == 1 ? "> No" : "  No";

        string fullMessage = $"{confirmationMessage}\n\n{yesOption}\n{noOption}";

        DialogueUI.Instance.ShowDialogue(confirmationTitle, fullMessage);
    }

    void ConfirmTeleport()
    {
        dialogueActive = false;
        playerInTrigger = false;

        // Hide dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }

        TeleportPlayer();
    }

    void CancelTeleport()
    {
        dialogueActive = false;
        playerInTrigger = false;

        // Hide dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }

        Debug.Log("Teleport cancelled");
    }

    void TeleportPlayer()
    {
        if (teleportDestination == null)
        {
            Debug.LogWarning($"TeleportTrigger '{triggerName}': No teleport destination set!");
            return;
        }

        // Get persistent player
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetPosition(teleportDestination.position);

            // Snap camera to avoid smooth follow across teleport
            if (snapCamera)
            {
                Camera mainCamera = Camera.main;
                if (mainCamera != null)
                {
                    Vector3 cameraPos = teleportDestination.position;
                    cameraPos.z = mainCamera.transform.position.z; // Keep camera's Z
                    mainCamera.transform.position = cameraPos;
                }
            }

            lastTeleportTime = Time.time;
            Debug.Log($"Player teleported via {triggerName}");
        }
        else
        {
            Debug.LogWarning($"TeleportTrigger '{triggerName}': Could not find PersistentPlayer!");
        }
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

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

        // Draw arrow to destination
        if (teleportDestination != null)
        {
            Gizmos.color = Color.cyan;
            Vector3 direction = (teleportDestination.position - transform.position).normalized;
            Gizmos.DrawLine(transform.position, teleportDestination.position);

            // Draw arrowhead
            Vector3 arrowTip = teleportDestination.position;
            Vector3 arrowBase = arrowTip - direction * 0.5f;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0) * 0.3f;
            Gizmos.DrawLine(arrowTip, arrowBase + perpendicular);
            Gizmos.DrawLine(arrowTip, arrowBase - perpendicular);

            // Draw destination sphere
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(teleportDestination.position, 0.5f);
        }
    }
}