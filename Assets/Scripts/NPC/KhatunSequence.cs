using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// Drives the full Khatun intro sequence:
/// Proximity trigger → Dialogue 1-3 → Moon Plane Reveal → Dialogue 4 → Boss Spawn
/// Player input is locked for the entire sequence until TriggerEntrance() is called.
/// </summary>
public class KhatunSequence : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string khatunName = "Khatun";
    [SerializeField] private Sprite khatunPortrait;
    
    [TextArea(2, 3)]
    [SerializeField] private string[] earlyLines = new string[]
    
    {
        "They have been watching us...",
        "The Tithe moves faster than I feared.",
        "Brace yourself. They will not let me leave."
    };
    [TextArea(2, 3)]
    [SerializeField] private string line4 = "The Radiant Plague has no mercy. There is no escape from this light.";

    [Header("Proximity")]
    [SerializeField] private float triggerRange = 3f;

    [Header("Reveal")]
    [SerializeField] private MoonPlaneReveal moonPlaneReveal;
    [SerializeField] private float delayBeforeReveal = 0.5f;  // Brief pause after line 3 before reveal starts
    [SerializeField] private float delayBeforeLine4 = 3f;     // How long player watches the reveal

    [Header("Colliders")]
    [SerializeField] private Tilemap swampCollision;
    [SerializeField] private Tilemap bossArenaCollision;

    [Header("Boss")]
    [SerializeField] private GameObject bossGameObject;

    // State
    private bool sequenceStarted = false;
    private int currentEarlyLine = 0;

    void Start()
    {
        // Boss starts inactive until TriggerEntrance
        if (bossGameObject != null)
            bossGameObject.SetActive(false);

        // Boss arena collision starts disabled
        if (bossArenaCollision != null)
            bossArenaCollision.gameObject.SetActive(false);
    }

    void Update()
    {
        if (sequenceStarted) return;

        // Proximity check
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= triggerRange)
            StartSequence();
    }

    void StartSequence()
    {
        sequenceStarted = true;

        // Lock player for the entire sequence
        if (PersistentInputManager.Instance != null)
            PersistentInputManager.Instance.SetForcedMovement(false);

        // Block all input
        BlockInput(true);

        StartCoroutine(SequenceCoroutine());
    }

    IEnumerator SequenceCoroutine()
    {
        // --- PHASE 1: Early dialogue (lines 1-3, player presses E to advance) ---
        currentEarlyLine = 0;
        ShowCurrentEarlyLine();

        // Wait for player to advance through all early lines
        while (currentEarlyLine < earlyLines.Length)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentEarlyLine++;

                if (currentEarlyLine < earlyLines.Length)
                    ShowCurrentEarlyLine();
                else
                    HideDialogue(); // Last early line read, close dialogue
            }
            yield return null;
        }

        // --- PHASE 2: Brief pause, then reveal ---
        yield return new WaitForSeconds(delayBeforeReveal);

        // Trigger the moon plane reveal
        if (moonPlaneReveal != null)
            moonPlaneReveal.TriggerReveal();

        // Swap colliders partway through the reveal
        // Half of delayBeforeLine4 feels natural — world is changing, boundaries shift
        float colliderSwapDelay = delayBeforeLine4 * 0.5f;
        yield return new WaitForSeconds(colliderSwapDelay);

        SwapColliders();

        // Wait for the remainder before line 4 appears
        yield return new WaitForSeconds(delayBeforeLine4 - colliderSwapDelay);

        // --- PHASE 3: Line 4 — tension before boss ---
        ShowDialogue(line4);

        // Wait for player to press E on line 4
        bool line4Advanced = false;
        while (!line4Advanced)
        {
            if (Input.GetKeyDown(KeyCode.E))
                line4Advanced = true;

            yield return null;
        }

        HideDialogue();

        // Brief beat after dialogue closes before boss appears
        yield return new WaitForSeconds(0.5f);

        // --- PHASE 4: Boss entrance ---
        TriggerBossEntrance();
    }

    void ShowCurrentEarlyLine()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowDialogue(khatunName, earlyLines[currentEarlyLine], khatunPortrait);
    }

    void ShowDialogue(string line)
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowDialogue(khatunName, line, khatunPortrait);
    }

    void HideDialogue()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.HideDialogue();
    }

    void SwapColliders()
    {
        if (swampCollision != null)
            swampCollision.gameObject.SetActive(false);

        if (bossArenaCollision != null)
            bossArenaCollision.gameObject.SetActive(true);

        Debug.Log("Colliders swapped — boss arena active");
    }

    void TriggerBossEntrance()
    {
        if (bossGameObject != null)
        {
            bossGameObject.SetActive(true);

            // Call TriggerEntrance on whatever boss script is on the GameObject
            BossBase boss = bossGameObject.GetComponent<BossBase>();
            if (boss != null)
                boss.TriggerEntrance();
            else
                Debug.LogWarning("KhatunSequence: No BossBase component found on boss GameObject");
        }

        // Unlock input — combat begins
        BlockInput(false);

        Debug.Log("Boss entrance triggered — player control restored");
    }

    void BlockInput(bool blocked)
    {
        // We drive input blocking manually through forced movement flag
        // Using a dedicated sequence block flag on PersistentInputManager
        if (PersistentInputManager.Instance != null)
            PersistentInputManager.Instance.SetSequenceBlocked(blocked);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}