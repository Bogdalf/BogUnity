using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Manages a sequential activation of RuneNode objects to create a trail of power.
/// Triggered by TriggerRune when fully charged.
///
/// Setup:
///   1. Place RuneNode objects in the scene along the desired path
///   2. Add them to the runeNodes list in order (first to last)
///   3. Assign what happens when the sequence completes via onSequenceComplete
///
/// The sequence can be used to open doors, trigger boss intros, change scene state, etc.
/// </summary>
public class RunePathSequence : MonoBehaviour
{
    [Header("Rune Path")]
    [Tooltip("Rune nodes in order from start to end of the path.")]
    [SerializeField] private List<RuneNode> runeNodes = new List<RuneNode>();

    [Header("Timing")]
    [SerializeField] private float delayBetweenRunes = 0.2f;  // Delay between each rune activating
    [SerializeField] private float delayBeforeEvent  = 0.5f;  // Pause after last rune before firing event

    [Header("On Complete")]
    [SerializeField] private UnityEvent onSequenceComplete;

    [Header("State")]
    [SerializeField] private bool canRepeat = false; // Can the sequence be triggered again?

    private bool isRunning   = false;
    private bool hasComplete = false;

    // ─── Public API ───────────────────────────────────────────────────────────────

    public void StartSequence()
    {
        if (isRunning) return;
        if (hasComplete && !canRepeat) return;

        if (runeNodes == null || runeNodes.Count == 0)
        {
            Debug.LogWarning("RunePathSequence: No rune nodes assigned!");
            return;
        }

        StartCoroutine(RunSequence());
    }

    public void ResetSequence()
    {
        StopAllCoroutines();
        isRunning   = false;
        hasComplete = false;

        foreach (RuneNode node in runeNodes)
            node?.Deactivate();
    }

    public bool IsRunning()   => isRunning;
    public bool IsComplete()  => hasComplete;

    // ─── Sequence ─────────────────────────────────────────────────────────────────

    IEnumerator RunSequence()
    {
        isRunning = true;
        Debug.Log($"Rune path sequence started — {runeNodes.Count} nodes.");

        for (int i = 0; i < runeNodes.Count; i++)
        {
            RuneNode node = runeNodes[i];
            if (node == null) continue;

            node.Activate();

            // Small delay between each rune activating
            if (delayBetweenRunes > 0f)
                yield return new WaitForSeconds(delayBetweenRunes);
        }

        // All runes activated — pause for dramatic effect before firing completion
        yield return new WaitForSeconds(delayBeforeEvent);

        isRunning   = false;
        hasComplete = true;

        Debug.Log("Rune path sequence complete!");
        onSequenceComplete?.Invoke();
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (runeNodes == null || runeNodes.Count < 2) return;

        // Draw lines connecting the rune path in the scene view
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.6f);
        for (int i = 0; i < runeNodes.Count - 1; i++)
        {
            if (runeNodes[i] == null || runeNodes[i + 1] == null) continue;
            Gizmos.DrawLine(runeNodes[i].transform.position, runeNodes[i + 1].transform.position);
        }

        // Draw spheres at each node
        Gizmos.color = new Color(0.7f, 0.7f, 1f, 0.8f);
        foreach (RuneNode node in runeNodes)
        {
            if (node != null)
                Gizmos.DrawWireSphere(node.transform.position, 0.3f);
        }
    }
}