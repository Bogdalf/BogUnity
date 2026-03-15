using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance;

    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Fade in on every scene load
        StartCoroutine(Fade(1f, 0f));
    }

    public void FadeToScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    public void FadeToScene(string sceneName, Vector3 spawnPosition)
    {
        StartCoroutine(FadeOutAndLoad(sceneName, spawnPosition));
    }

    IEnumerator FadeOutAndLoad(string sceneName, Vector3? spawnPosition = null)
    {
        yield return StartCoroutine(Fade(0f, 1f));

        if (PersistentUICanvas.Instance != null)
            PersistentUICanvas.Instance.CloseAllPanels();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
            yield return null;

        if (spawnPosition.HasValue && PersistentPlayer.Instance != null)
            PersistentPlayer.Instance.SetPosition(spawnPosition.Value);

        // Fade back in
        yield return StartCoroutine(Fade(1f, 0f));
    }

    IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        float elapsed = 0f;
        SetFadeAlpha(fromAlpha);
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetFadeAlpha(Mathf.Lerp(fromAlpha, toAlpha, elapsed / fadeDuration));
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