using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Interactive door that prompts player to transition to another scene.
/// Shows a simple Yes/No dialogue using the DialogueUI system.
/// </summary>
public class InteractiveTable : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private string doorName = "Inn Entrance";
    [SerializeField] private string targetSceneName = "WorldMap";
    [SerializeField] private string promptMessage = "Open World Map?";

    [Header("Choice Settings")]
    [SerializeField] private KeyCode acceptKey = KeyCode.E;
    [SerializeField] private KeyCode selectUpKey = KeyCode.W;
    [SerializeField] private KeyCode selectDownKey = KeyCode.S;

    private bool playerInRange = false;
    private bool dialogueActive = false;
    private int currentSelection = 0; // 0 = Yes, 1 = No

    void Update()
    {
        if (!playerInRange) return;

        if (!dialogueActive)
        {
            // Show initial prompt
            if (Input.GetKeyDown(acceptKey))
            {
                ShowChoiceDialogue();
            }
        }
        else
        {
            // Handle choice selection
            HandleChoiceInput();
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log($"Press {acceptKey} to interact with {doorName}");
            // TODO: Show UI prompt
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueActive = false;

            // Hide dialogue
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideDialogue();
            }
        }
    }

    void ShowChoiceDialogue()
    {
        dialogueActive = true;
        currentSelection = 0; // Default to "Yes"
        UpdateDialogueDisplay();
    }

    void HandleChoiceInput()
    {
        // Navigate between choices
        if (Input.GetKeyDown(selectUpKey) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection = 0; // Select "Yes"
            UpdateDialogueDisplay();
        }
        else if (Input.GetKeyDown(selectDownKey) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = 1; // Select "No"
            UpdateDialogueDisplay();
        }

        // Confirm choice
        if (Input.GetKeyDown(acceptKey))
        {
            if (currentSelection == 0)
            {
                // Yes - Load scene
                LoadTargetScene();
            }
            else
            {
                // No - Cancel
                CancelDialogue();
            }
        }
    }

    void UpdateDialogueDisplay()
    {
        if (DialogueUI.Instance == null) return;

        string yesOption = currentSelection == 0 ? "> Yes" : "  Yes";
        string noOption = currentSelection == 1 ? "> No" : "  No";

        string fullMessage = $"{promptMessage}\n\n{yesOption}\n{noOption}";

        DialogueUI.Instance.ShowDialogue(doorName, fullMessage);
    }

    void LoadTargetScene()
    {
        dialogueActive = false;

        // Hide dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }

        // Close any open UI
        if (PersistentUICanvas.Instance != null)
        {
            PersistentUICanvas.Instance.CloseAllPanels();
        }

        Debug.Log($"Loading scene: {targetSceneName}");
        SceneManager.LoadScene(targetSceneName);
    }

    void CancelDialogue()
    {
        dialogueActive = false;

        // Hide dialogue
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }

        Debug.Log("Scene transition cancelled");
    }

    // Debug visualization
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

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
    }
}