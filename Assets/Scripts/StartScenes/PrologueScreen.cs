using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Displays context paragraphs one at a time, full screen.
/// Player clicks Continue to advance. Final paragraph shows "Start Game" instead.
/// Transitions to the next scene when Start Game is clicked.
/// </summary>
public class PrologueScreen : MonoBehaviour
{
    [Header("Content")]
    [TextArea(4, 10)]
    [SerializeField] private string[] paragraphs = new string[]
    {
        "Placeholder paragraph one.",
        "Placeholder paragraph two.",
        "Placeholder paragraph three."
    };

    [Header("Scene Transition")]
    [SerializeField] private string nextSceneName;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI paragraphText;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI continueButtonText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Timing")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float fadeOutDuration = 0.6f;

    private int currentIndex = 0;

    void Start()
    {
        continueButton.onClick.AddListener(OnContinue);
        ShowParagraph(0);
    }

    void ShowParagraph(int index)
    {
        paragraphText.text = paragraphs[index];

        bool isLast = index >= paragraphs.Length - 1;
        continueButtonText.text = isLast ? "Start Game" : "Continue";

        StartCoroutine(FadeIn());
    }

    void OnContinue()
    {
        if (currentIndex >= paragraphs.Length - 1)
        {
            // Last paragraph — start the game
            StartCoroutine(StartGame());
        }
        else
        {
            StartCoroutine(AdvanceParagraph());
        }
    }

    IEnumerator AdvanceParagraph()
    {
        continueButton.interactable = false;

        yield return StartCoroutine(FadeOut());

        currentIndex++;
        ShowParagraph(currentIndex);

        continueButton.interactable = true;
    }

    IEnumerator StartGame()
    {
        continueButton.interactable = false;

        yield return StartCoroutine(FadeOut());

        if (SceneFader.Instance != null)
            SceneFader.Instance.FadeToScene(nextSceneName);
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
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

    IEnumerator FadeOut()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}