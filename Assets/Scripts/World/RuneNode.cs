using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A single rune node in a pathway sequence.
/// Placed manually in the scene and assigned to RunePathSequence in order.
/// Starts dim/inactive and activates with a visual flourish when triggered.
/// </summary>
public class RuneNode : MonoBehaviour
{
    [Header("Companions")]
    [Tooltip("Objects that activate at the same time as this rune. " +
            "Can be pillars, lights, particle systems, anything with an Activate() method " +
            "or just a GameObject to enable.")]
    [SerializeField] private List<GameObject> companionObjects = new List<GameObject>();
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer runeRenderer;
    [SerializeField] private Color inactiveColor = new Color(0.3f, 0.3f, 0.5f, 0.4f);
    [SerializeField] private Color activeColor   = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField] private Color pulseColor    = new Color(1f, 1f, 1f, 1f);

    [Header("Animation")]
    [SerializeField] private float activateDuration = 0.3f;  // Time to flash to full brightness
    [SerializeField] private float pulseSpeed       = 2f;    // Pulse rate once active
    [SerializeField] private bool  pulseWhenActive  = true;

    [Header("VFX")]
    [SerializeField] private ParticleSystem activateParticles;

    private bool isActive = false;

    void Awake()
    {
        if (runeRenderer == null)
            runeRenderer = GetComponent<SpriteRenderer>();

        // Start inactive
        if (runeRenderer != null)
            runeRenderer.color = inactiveColor;
    }

    void Update()
    {
        if (!isActive || !pulseWhenActive) return;

        // Gentle pulse once active
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        if (runeRenderer != null)
            runeRenderer.color = Color.Lerp(activeColor, pulseColor, pulse * 0.3f);
    }

    // ─── Public API ───────────────────────────────────────────────────────────────

    public void Activate()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(ActivateSequence());
        foreach (GameObject companion in companionObjects)
        {
            if (companion != null)
            {
                Debug.Log($"Activating companion: {companion.name}");
                companion.SetActive(true);
            }
            else
            {
                Debug.Log("Companion is null!");
            }
        }
    }

    public void Deactivate()
    {
        isActive = false;
        StopAllCoroutines();
        if (runeRenderer != null)
            runeRenderer.color = inactiveColor;
    }

    public bool IsActive() => isActive;

    // ─── Activate Sequence ────────────────────────────────────────────────────────

    IEnumerator ActivateSequence()
    {
        // Flash bright then settle to active color
        float elapsed = 0f;
        Color startColor = inactiveColor;

        while (elapsed < activateDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / activateDuration;

            // Flash to white then settle to active color
            Color flashColor = Color.Lerp(startColor, pulseColor, Mathf.Sin(t * Mathf.PI));
            Color settleColor = Color.Lerp(startColor, activeColor, t);
            if (runeRenderer != null)
                runeRenderer.color = Color.Lerp(flashColor, settleColor, t);

            yield return null;
        }

        if (runeRenderer != null)
            runeRenderer.color = activeColor;

        // Play particle burst
        if (activateParticles != null)
            activateParticles.Play();
    }
}