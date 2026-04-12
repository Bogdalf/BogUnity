using UnityEngine;
using System.Collections;

/// <summary>
/// Drives the vision/intermission scene.
/// Dialogue begins automatically on load, player presses E to advance lines,
/// then the screen fades to black and transitions to the next scene.
///
/// The crimson overlay from SunPriestessInterjection carries over naturally —
/// no additional setup needed for the tint.
/// </summary>
public class VisionSequence : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip loopClip;
    [SerializeField] private AudioSource musicSource;
    [Header("Dialogue")]
    [SerializeField] private string speakerName = "???";
    [SerializeField] private Sprite speakerPortrait;
    [TextArea(2, 4)]
    [SerializeField] private string[] lines = new string[]
    {
        "You should not be here.",
        "And yet... here you are."
    };

    [Header("Timing")]
    [SerializeField] private float delayBeforeDialogue = 1.5f;

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    void Start()
    {
       // Ensure dialogue renders above the crimson overlay
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.transform.SetAsLastSibling();
        // Hide the player while in the vision
        if (PersistentPlayer.Instance != null)
            PersistentPlayer.Instance.gameObject.SetActive(false);

        StartCoroutine(VisionCoroutine());
    }

    IEnumerator VisionCoroutine()
    {
        if (musicSource !=null && introClip !=null)
            StartCoroutine(PlayIntroThenLoop());
        
        // Brief pause before first line — lets the scene settle
        yield return new WaitForSeconds(delayBeforeDialogue);

        // --- Dialogue ---
        for (int i = 0; i < lines.Length; i++)
        {
            ShowDialogue(lines[i]);

            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.E));
            yield return new WaitForSeconds(0.1f);
        }

        HideDialogue();

        // Brief beat after final line before fading
        yield return new WaitForSeconds(0.8f);

        if (musicSource !=null)
            StartCoroutine(FadeOutMusic(SceneFader.Instance != null ? 1f : 0.3f));

        GameObject overlay = GameObject.Find("SunPriestessWhiteOverlay");
        if (overlay != null)
            Destroy(overlay);

        // --- Fade to black and transition ---
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneFader.Instance != null)
            {
                // Restore player before scene loads
                if (PersistentPlayer.Instance != null)
                    PersistentPlayer.Instance.gameObject.SetActive(true);

                SceneFader.Instance.FadeToScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[VisionSequence] No SceneFader found — loading scene directly.");
                UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
            }
        }
    }
    IEnumerator PlayIntroThenLoop()
    {
        musicSource.clip = introClip;
        musicSource.loop = false;
        musicSource.Play();

        while (musicSource.timeSamples < introClip.samples - 1)
            yield return null;

        if (loopClip != null)
        {
            musicSource.clip = loopClip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
    IEnumerator FadeOutMusic(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
        musicSource.volume = startVolume;
    }
    void ShowDialogue(string line)
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.ShowDialogue(speakerName, line, speakerPortrait);
    }

    void HideDialogue()
    {
        if (DialogueUI.Instance != null)
            DialogueUI.Instance.HideDialogue();
    }
}