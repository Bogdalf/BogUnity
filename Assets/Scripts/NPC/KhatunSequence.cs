using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

/// <summary>
/// Drives the Khatun intro sequence:
/// Proximity trigger → Dialogue 1-3 → Moon Plane Reveal → Dialogue 4 → Combat begins
/// Player input and enemy AI are locked for the entire sequence until line 4 is dismissed.
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
    [SerializeField] private float delayBeforeReveal = 0.5f;
    [SerializeField] private float delayBeforeLine4 = 3f;

    [Header("Colliders")]
    [SerializeField] private Tilemap swampCollision;
    [SerializeField] private Tilemap bossArenaCollision;

    private bool sequenceStarted = false;
    private int currentEarlyLine = 0;

    void Start()
    {
        if (bossArenaCollision != null)
            bossArenaCollision.gameObject.SetActive(false);
    }

    void Update()
    {
        if (sequenceStarted) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        if (distance <= triggerRange)
            StartSequence();
    }

    void StartSequence()
    {
        sequenceStarted = true;
        BlockInput(true);
        CombatPauseManager.SetPaused(true);
        StartCoroutine(SequenceCoroutine());
    }

    IEnumerator SequenceCoroutine()
    {
        // --- PHASE 1: Early dialogue (lines 1-3) ---
        currentEarlyLine = 0;
        ShowCurrentEarlyLine();

        while (currentEarlyLine < earlyLines.Length)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                currentEarlyLine++;

                if (currentEarlyLine < earlyLines.Length)
                    ShowCurrentEarlyLine();
                else
                    HideDialogue();
            }
            yield return null;
        }

        // --- PHASE 2: Brief pause, then reveal ---
        yield return new WaitForSeconds(delayBeforeReveal);

        if (moonPlaneReveal != null)
            moonPlaneReveal.TriggerReveal();

        // Swap colliders halfway through the reveal window
        float colliderSwapDelay = delayBeforeLine4 * 0.5f;
        yield return new WaitForSeconds(colliderSwapDelay);

        SwapColliders();

        yield return new WaitForSeconds(delayBeforeLine4 - colliderSwapDelay);

        // --- PHASE 3: Line 4 ---
        ShowDialogue(line4);

        while (!Input.GetKeyDown(KeyCode.E))
            yield return null;

        HideDialogue();

        // --- Unpause — combat begins ---
        CombatPauseManager.SetPaused(false);
        BlockInput(false);
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
    }

    void BlockInput(bool blocked)
    {
        if (PersistentInputManager.Instance != null)
            PersistentInputManager.Instance.SetSequenceBlocked(blocked);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, triggerRange);
    }
}