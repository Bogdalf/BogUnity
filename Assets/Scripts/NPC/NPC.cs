using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour, IInteractable
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "Villager";

    [Header("Dialogue")]
    [SerializeField] private bool proximityTrigger = false;
    [TextArea(3, 10)]
    [SerializeField]
    private string[] dialogueLines = new string[]
    {
        "Hello, traveler!",
        "Welcome to our village.",
        "Be careful out there!"
    };

    [Header("On Dialogue Complete")]
    [SerializeField] private UnityEvent onDialogueComplete; // Fires when last line is read

    [Header("Post Dialogue Movement")]
    [SerializeField] private bool moveAfterDialogue = false;
    [SerializeField] private Transform[] moveTargets;
    [SerializeField] private float moveSpeed = 2f;

    private int currentDialogueIndex = 0;
    private int currentTargetIndex = 0;
    private bool isPlayerNearby = false;
    private bool dialogueCompleted = false;
    private bool isMoving = false;
    private GameObject player;

    void Update()
    {
        if (isMoving && moveTargets != null && moveTargets.Length > 0)
        {
            Transform currentTarget = moveTargets[currentTargetIndex];

            transform.position = Vector2.MoveTowards(
                transform.position,
                currentTarget.position,
                moveSpeed * Time.deltaTime
            );

            float distance = Vector2.Distance(
                new Vector2(transform.position.x, transform.position.y),
                new Vector2(currentTarget.position.x, currentTarget.position.y)
            );

            if (distance < 0.05f)
            {
                transform.position = new Vector3(
                    currentTarget.position.x,
                    currentTarget.position.y,
                    transform.position.z
                );

                currentTargetIndex++;

                if (currentTargetIndex >= moveTargets.Length)
                {
                    isMoving = false;
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !dialogueCompleted)
        {
            isPlayerNearby = true;
            player = collision.gameObject;

            if (proximityTrigger)
                TriggerDialogue();
            else
                Debug.Log("Press E to talk to " + npcName);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            player = null;

            if (!dialogueCompleted && DialogueUI.Instance != null)
            {
                DialogueUI.Instance.HideDialogue();
                currentDialogueIndex = 0;
            }
        }
    }

    void TriggerDialogue()
    {
        if (DialogueUI.Instance != null && dialogueLines.Length > 0)
        {
            DialogueUI.Instance.ShowDialogue(npcName, dialogueLines[currentDialogueIndex]);
            currentDialogueIndex++;
        }
    }

    public void Interact()
    {
        if (dialogueCompleted || !isPlayerNearby) return;

        if (DialogueUI.Instance != null)
        {
            if (currentDialogueIndex < dialogueLines.Length)
            {
                DialogueUI.Instance.ShowDialogue(npcName, dialogueLines[currentDialogueIndex]);
                currentDialogueIndex++;
            }
            else
            {
                // Last line read — fire event first, then handle movement
                DialogueUI.Instance.HideDialogue();
                dialogueCompleted = true;
                onDialogueComplete?.Invoke();

                if (moveAfterDialogue && moveTargets != null && moveTargets.Length > 0)
                    isMoving = true;
            }
        }
    }

    public bool IsPlayerNearby() => isPlayerNearby;
    public string GetNPCName() => npcName;
}