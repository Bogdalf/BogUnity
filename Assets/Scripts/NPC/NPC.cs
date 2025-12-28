using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Villager";

    [Header("Dialogue")]
    [TextArea(3, 10)]
    [SerializeField]
    private string[] dialogueLines = new string[]
    {
        "Hello, traveler!",
        "Welcome to our village.",
        "Be careful out there!"
    };

    private int currentDialogueIndex = 0;
    private bool isPlayerNearby = false;
    private GameObject player;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            player = collision.gameObject;

            // Show interaction prompt (optional - you can add UI for this later)
            Debug.Log("Press E to talk to " + npcName);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;

            // Hide dialogue if player walks away
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideDialogue();
            }
        }
    }

    public void Interact()
    {
        if (DialogueUI.Instance != null)
        {
            // If we're at the end of dialogue and it's showing, close it
            if (currentDialogueIndex == dialogueLines.Length && DialogueUI.Instance.IsDialogueShowing())
            {
                DialogueUI.Instance.HideDialogue();
                currentDialogueIndex = 0; // Reset to start
                return;
            }

            // Show the current dialogue line
            DialogueUI.Instance.ShowDialogue(npcName, dialogueLines[currentDialogueIndex]);

            // Move to next line
            currentDialogueIndex++;

            // If we've shown all lines, the next press will close it
        }
    }

    public bool IsPlayerNearby()
    {
        return isPlayerNearby;
    }

    public string GetNPCName()
    {
        return npcName;
    }
}