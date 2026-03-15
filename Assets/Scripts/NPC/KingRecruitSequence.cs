using UnityEngine;
using UnityEngine.SceneManagement;

public class KingRecruitSequence : MonoBehaviour, IInteractable
{
    [Header("Dialogue - Opening")]
    [SerializeField] private string npcName = "The King";
    [TextArea(3, 10)]
    [SerializeField] private string[] openingLines = new string[]
    {
        "You fought well back there.",
        "I need someone like you. Will you accompany me to the capital?",
        "Are you ready to leave?"
    };

    [Header("Dialogue - Refused")]
    [TextArea(3, 10)]
    [SerializeField] private string[] refusedLines = new string[]
    {
        "Take your time. I will be here when you are ready."
    };

    [Header("Dialogue - Arrival")]
    [TextArea(3, 10)]
    [SerializeField] private string[] arrivalLines = new string[]
    {
        "There it is. The capital.",
        "Something is very wrong."
    };

    [Header("Movement")]
    [SerializeField] private Transform[] kingWaypoints;
    [SerializeField] private Transform[] playerWaypoints;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float playerFollowDelay = 0.8f; // Seconds behind king

    [Header("Scene Transition")]
    [SerializeField] private string targetScene = "";
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private bool useSpawnPoint = true;

    // State
    private enum SequenceState
    {
        Idle,
        OpeningDialogue,
        YesNoChoice,
        Refused,
        Walking,
        ArrivalDialogue,
        Done
    }

    public bool IsPlayerNearby() => isPlayerNearby;
    private SequenceState state = SequenceState.Idle;
    private bool isPlayerNearby = false;
    private int dialogueIndex = 0;
    private int currentSelection = 0; // 0 = Yes, 1 = No
    private int kingWaypointIndex = 0;
    private int playerWaypointIndex = 0;
    private float playerFollowTimer = 0f;
    private bool playerFollowStarted = false;
    private int arrivalDialogueIndex = 0;

    void Update()
    {
        switch (state)
        {
            case SequenceState.OpeningDialogue:
                HandleOpeningInput();
                break;

            case SequenceState.YesNoChoice:
                HandleYesNoInput();
                break;

            case SequenceState.Refused:
                HandleRefusedInput();
                break;

            case SequenceState.Walking:
                HandleWalking();
                break;

            case SequenceState.ArrivalDialogue:
                HandleArrivalInput();
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    // Called by your existing InputManager/interaction system when E is pressed
    public void Interact()
    {
        if (!isPlayerNearby || state == SequenceState.Done) return;

        if (state == SequenceState.Idle)
        {
            StartOpeningDialogue();
        }
    }

    // ── OPENING DIALOGUE ──

    void StartOpeningDialogue()
    {
        state = SequenceState.OpeningDialogue;
        dialogueIndex = 0;
        ShowLine(openingLines[dialogueIndex]);
        dialogueIndex++;
    }

    void HandleOpeningInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (dialogueIndex < openingLines.Length)
        {
            ShowLine(openingLines[dialogueIndex]);
            dialogueIndex++;
        }
        else
        {
            // All opening lines shown — move to Yes/No
            ShowYesNo();
        }
    }

    // ── YES/NO ──

    void ShowYesNo()
    {
        state = SequenceState.YesNoChoice;
        currentSelection = 0;
        UpdateYesNoDisplay();
    }

    void HandleYesNoInput()
    {
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            currentSelection = 0;
            UpdateYesNoDisplay();
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            currentSelection = 1;
            UpdateYesNoDisplay();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentSelection == 0)
                OnPlayerSaidYes();
            else
                OnPlayerSaidNo();
        }
    }

    void UpdateYesNoDisplay()
    {
        string yes = currentSelection == 0 ? "> Yes" : "  Yes";
        string no  = currentSelection == 1 ? "> No"  : "  No";
        DialogueUI.Instance.ShowDialogue(npcName, $"Are you ready to leave?\n\n{yes}\n{no}");
    }

    // ── REFUSED ──

    void OnPlayerSaidNo()
    {
        state = SequenceState.Refused;
        dialogueIndex = 0;
        ShowLine(refusedLines[dialogueIndex]);
        dialogueIndex++;
    }

    void HandleRefusedInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (dialogueIndex < refusedLines.Length)
        {
            ShowLine(refusedLines[dialogueIndex]);
            dialogueIndex++;
        }
        else
        {
            // Reset back to idle so player can try again
            DialogueUI.Instance.HideDialogue();
            state = SequenceState.Idle;
        }
    }

    // ── WALKING ──

    void OnPlayerSaidYes()
    {
        DialogueUI.Instance.HideDialogue();
        state = SequenceState.Walking;
        kingWaypointIndex = 0;
        playerWaypointIndex = 0;
        playerFollowTimer = 0f;
        playerFollowStarted = false;

        // Block player input, enable forced movement
        PersistentInputManager.Instance.SetForcedMovement(true);
    }

    void HandleWalking()
    {
        MoveKing();
        MovePlayer();

        // Check if both have finished all waypoints
        bool kingDone = kingWaypointIndex >= kingWaypoints.Length;
        bool playerDone = playerWaypointIndex >= playerWaypoints.Length;

        if (kingDone && playerDone)
        {
            StartArrivalDialogue();
        }
    }

    void MoveKing()
    {
        if (kingWaypointIndex >= kingWaypoints.Length) return;

        Transform target = kingWaypoints[kingWaypointIndex];
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(target.position.x, target.position.y)) < 0.05f)
        {
            transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);
            kingWaypointIndex++;
        }
    }

    void MovePlayer()
    {
        if (PersistentPlayer.Instance == null) return;

        if (!playerFollowStarted)
        {
            playerFollowTimer += Time.deltaTime;
            if (playerFollowTimer >= playerFollowDelay)
                playerFollowStarted = true;
            else
                return;
        }

        if (playerWaypointIndex >= playerWaypoints.Length) return;

        Transform target = playerWaypoints[playerWaypointIndex];
        Vector3 playerPos = PersistentPlayer.Instance.transform.position;
        Vector3 newPos = Vector2.MoveTowards(playerPos, target.position, moveSpeed * Time.deltaTime);

        // Drive animator
        Animator playerAnimator = PersistentPlayer.Instance.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            Vector2 direction = ((Vector3)target.position - playerPos).normalized;
            float distance = Vector2.Distance(playerPos, target.position);

            if (distance > 0.05f)
            {
                playerAnimator.SetFloat("Speed", 1f);
                playerAnimator.SetFloat("MovementX", direction.x);
                playerAnimator.SetFloat("MovementY", direction.y);
            }
        }

        PersistentPlayer.Instance.transform.position = newPos;

        if (Vector2.Distance(new Vector2(newPos.x, newPos.y),
            new Vector2(target.position.x, target.position.y)) < 0.05f)
        {
            PersistentPlayer.Instance.transform.position = new Vector3(
                target.position.x, target.position.y, playerPos.z);
            playerWaypointIndex++;
        }
    }

    // ── ARRIVAL DIALOGUE ──

    void StartArrivalDialogue()
    {
        // Stop forced movement
        PersistentInputManager.Instance.SetForcedMovement(false);

        // Zero out animator
        if (PersistentPlayer.Instance != null)
        {
            Animator playerAnimator = PersistentPlayer.Instance.GetComponent<Animator>();
            if (playerAnimator != null)
            {
                playerAnimator.SetFloat("Speed", 0);
            }
        }

        state = SequenceState.ArrivalDialogue;
        arrivalDialogueIndex = 0;
        ShowLine(arrivalLines[arrivalDialogueIndex]);
        arrivalDialogueIndex++;
    }
    void HandleArrivalInput()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        if (arrivalDialogueIndex < arrivalLines.Length)
        {
            ShowLine(arrivalLines[arrivalDialogueIndex]);
            arrivalDialogueIndex++;
        }
        else
        {
            DialogueUI.Instance.HideDialogue();
            state = SequenceState.Done;
            TriggerSceneTransition();
        }
    }

    // ── SCENE TRANSITION ──

    void TriggerSceneTransition()
    {
        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("KingRecruitSequence: No target scene set!");
            return;
        }

        if (PersistentUICanvas.Instance != null)
            PersistentUICanvas.Instance.CloseAllPanels();

        if (useSpawnPoint)
        {
            SceneFader.Instance.FadeToScene(targetScene);
        }
        else
        {
            StartCoroutine(LoadSceneAndSetPosition());
        }
    }

    System.Collections.IEnumerator LoadSceneAndSetPosition()
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
        while (!asyncLoad.isDone)
            yield return null;

        if (PersistentPlayer.Instance != null)
            PersistentPlayer.Instance.SetPosition(spawnPosition);
    }

    // ── HELPERS ──

    void ShowLine(string line)
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowDialogue(npcName, line);
    }
}