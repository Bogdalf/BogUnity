using UnityEngine;
using System.Collections;

/// <summary>
/// Plays an intro clip once, then seamlessly switches to a looping clip.
/// Call Play() from any UnityEvent (e.g. NPC.onDialogueComplete).
/// </summary>
public class MusicPlayer : MonoBehaviour
{
    
    [SerializeField] private AudioSource audioSource;
    [Header("Clips")]
    [SerializeField] private AudioClip introClip;
    [SerializeField] private AudioClip loopClip;

    [Header("Settings")]
    [SerializeField] private float fadeInDuration = 1f;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool playOnAwake = false;


    void Awake()
    {
        audioSource.playOnAwake = false;
        audioSource.volume = volume;

        if (playOnAwake)
            Play();
    }

    /// <summary>
    /// Call this from a UnityEvent to start the intro → loop sequence.
    /// </summary>
    public void Play()
    {
        StartCoroutine(PlaySequence());
        
    }

    public void Stop()
    {
        StopAllCoroutines();
        audioSource.Stop();
    }
    
    public void FadeOut(float duration = 1f)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    IEnumerator PlaySequence()
    {
        audioSource.volume = 0f;
        if (introClip !=null)
        {
            audioSource.clip = introClip;
            audioSource.loop = false;
            audioSource.Play();
        }
        
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, volume, elapsed / fadeInDuration);
            yield return null;
        }
        audioSource.volume = volume;

        if (introClip != null)
            yield return new WaitForSeconds(introClip.length);
        
        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }
}