using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class CutscenePlayer : MonoBehaviour
{
    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string nextScene;

    [Header("Fade")]
    [SerializeField] private Image fadeImage; // A black UI Image that covers the screen
    [SerializeField] private float fadeDuration = 1f;

    void Start()
    {
        if (PersistentUICanvas.Instance != null)
            PersistentUICanvas.Instance.gameObject.SetActive(false);

        if (PersistentPlayer.Instance != null)
            PersistentPlayer.Instance.gameObject.SetActive(false);

        StartCoroutine(PlaySequence());
    }

    IEnumerator PlaySequence()
    {
        // Start fully black
        SetFadeAlpha(1f);

        // Wait for video player to be ready
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return null;

        // Fade in
        yield return StartCoroutine(Fade(1f, 0f));

        // Play video
        videoPlayer.Play();
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(FadeOutAndLoad());
    }

    IEnumerator FadeOutAndLoad()
    {
        // Fade out
        yield return StartCoroutine(Fade(0f, 1f));

        if (PersistentUICanvas.Instance != null)
            PersistentUICanvas.Instance.gameObject.SetActive(true);

        if (PersistentPlayer.Instance != null)
            PersistentPlayer.Instance.gameObject.SetActive(true);

        SceneFader.Instance.FadeToScene(nextScene);
    }

    IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration);
            SetFadeAlpha(alpha);
            yield return null;
        }
        SetFadeAlpha(toAlpha);
    }

    void SetFadeAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color c = fadeImage.color;
            c.a = alpha;
            fadeImage.color = c;
        }
    }
}