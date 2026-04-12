using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles the Sun Priestess interjection sequence that ends the intro scene.
///
/// Flow:
/// 1. Call BeginCountdown() when MoonPlaneReveal fires — starts 30 second timer
/// 2. Timer expires → pauses all enemies and player input
/// 3. Sun Priestess dialogue plays (player presses E to advance)
/// 4. After final line → full-screen white bloom expands to fill screen
/// 5. At full white → SceneFader triggers next scene
///
/// The white overlay Image is created at runtime as a child of PersistentUICanvas,
/// so no cross-scene Inspector reference is needed.
/// </summary>
public class SunPriestessInterjection : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private MusicPlayer musicPlayer;[Header("Timing")]
    [SerializeField] private float combatDuration = 30f;

    [Header("Dialogue")]
    [SerializeField] private string sunPriestessName = "???";
    [SerializeField] private Sprite sunPriestessPortrait;
    [TextArea(2, 4)]
    [SerializeField] private string[] lines = new string[]
    {
        "Enough.",
        "You have seen what you needed to see.",
        "Sleep now. We will speak again when the light fades."
    };

    [Header("White Flash")]
    [SerializeField] private float flashDuration = 2f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    private bool started = false;
    private bool sequenceRunning = false;
    private Image whiteOverlay;

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Call this from MoonPlaneReveal at the moment the rift opens.
    /// Starts the combat window timer.
    /// </summary>
    public void BeginCountdown()
    {
        if (started) return;
        started = true;
        StartCoroutine(CombatWindowCoroutine());
    }

    // -----------------------------------------------------------------------
    // Coroutines
    // -----------------------------------------------------------------------

    IEnumerator CombatWindowCoroutine()
    {
        yield return new WaitForSeconds(combatDuration);

        if (!sequenceRunning)
            StartCoroutine(InterjectionSequence());
    }

    IEnumerator InterjectionSequence()
    {
        sequenceRunning = true;

        // --- Pause everything ---
        CombatPauseManager.SetPaused(true);

        if (PersistentInputManager.Instance != null)
            PersistentInputManager.Instance.SetSequenceBlocked(true);

        yield return new WaitForSeconds(0.5f);

        // --- Dialogue ---
        for (int i = 0; i < lines.Length; i++)
        {
            ShowDialogue(lines[i]);

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            yield return new WaitForSeconds(0.1f);
        }

        HideDialogue();

        yield return new WaitForSeconds(0.6f);

        if (musicPlayer !=null)
            musicPlayer.FadeOut(flashDuration);

        // --- White flash ---
        CreateWhiteOverlay();
        yield return StartCoroutine(WhiteFlash());

        // --- Scene transition ---
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneFader.Instance != null)
                SceneFader.Instance.FadeToScene(nextSceneName);
            else
            {
                Debug.LogWarning("[SunPriestessInterjection] No SceneFader found — loading scene directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }

    IEnumerator WhiteFlash()
    {
        if (whiteOverlay == null) yield break;

        SetOverlayAlpha(0f);
        whiteOverlay.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / flashDuration);
            SetOverlayAlpha(Mathf.Pow(t, 0.6f));
            yield return null;
        }

        SetOverlayAlpha(1f);
        yield return new WaitForSeconds(0.3f);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a full-screen Image as a child of PersistentUICanvas at runtime.
    /// Avoids the cross-scene serialization issue entirely.
    /// </summary>
    void CreateWhiteOverlay()
    {
        if (whiteOverlay != null) return;

        Transform canvasTransform = PersistentUICanvas.Instance != null
            ? PersistentUICanvas.Instance.transform
            : FindFirstObjectByType<Canvas>()?.transform;

        if (canvasTransform == null)
        {
            Debug.LogWarning("[SunPriestessInterjection] No Canvas found to attach white overlay to!");
            return;
        }

        GameObject overlayGO = new GameObject("SunPriestessWhiteOverlay");
        overlayGO.transform.SetParent(canvasTransform, false);

        // Stretch to fill the entire canvas
        RectTransform rect = overlayGO.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // Render on top of everything else
        Transform dialogueTransform = DialogueUI.Instance != null 
            ? DialogueUI.Instance.transform 
            : null;

        if (dialogueTransform != null)
            overlayGO.transform.SetSiblingIndex(dialogueTransform.GetSiblingIndex());
        else
            overlayGO.transform.SetAsLastSibling();

        whiteOverlay = overlayGO.AddComponent<Image>();
        whiteOverlay.color = new Color(.34f, 0f, 0f, 0.086f);
        whiteOverlay.raycastTarget = false;

        overlayGO.SetActive(false);
    }

    void ShowDialogue(string line)
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowDialogue(sunPriestessName, line, sunPriestessPortrait);
    }

    void HideDialogue()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.HideDialogue();
    }

    void SetOverlayAlpha(float alpha)
    {
        if (whiteOverlay == null) return;
        Color c = whiteOverlay.color;
        c.a = alpha;
        whiteOverlay.color = c;
    }
}