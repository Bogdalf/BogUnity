using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Drives the title screen. New Game transitions to the prologue scene.
/// Continue and Options are stubs for future implementation.
/// </summary>
public class TitleScreen : MonoBehaviour
{
    [Header("Scene Transition")]
    [SerializeField] private string prologueSceneName = "Prologue";

    [Header("UI References")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;

    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 1.5f;
    [SerializeField] private CanvasGroup canvasGroup;

    void Start()
    {
        newGameButton.onClick.AddListener(OnNewGame);
        continueButton.onClick.AddListener(OnContinue);
        optionsButton.onClick.AddListener(OnOptions);

        // Continue greyed out until save system exists
        continueButton.interactable = false;

        if (canvasGroup != null)
            StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    void OnNewGame()
    {
        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(prologueSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(prologueSceneName);
    }

    void OnContinue()
    {
        // Stub — wire up save system later
        Debug.Log("[TitleScreen] Continue — not yet implemented.");
    }

    void OnOptions()
    {
        // Stub — wire up options panel later
        Debug.Log("[TitleScreen] Options — not yet implemented.");
    }
}