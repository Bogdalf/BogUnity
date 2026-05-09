using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The rune the player stands on to charge and trigger the pathway sequence.
/// Requires a CircleCollider2D set to Is Trigger.
///
/// Shows a charge progress indicator while the player stands on it.
/// On full charge, notifies RunePathSequence to begin.
/// If the player leaves before full charge, the rune resets.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TriggerRune : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeDuration = 3f;  // Seconds to fully charge
    [SerializeField] private bool  resetOnLeave   = true; // Reset if player steps off

    [Header("Visuals")]
    [SerializeField] private SpriteRenderer runeRenderer;
    [SerializeField] private Color idleColor      = new Color(0.4f, 0.3f, 0.6f, 0.5f);
    [SerializeField] private Color chargingColor  = new Color(0.6f, 0.5f, 1f, 0.8f);
    [SerializeField] private Color chargedColor   = new Color(1f, 0.9f, 0.5f, 1f);

    [Header("Charge Ring (Optional)")]
    [Tooltip("A radial fill Image that shows charge progress. Leave empty to skip.")]
    [SerializeField] private Image chargeRingImage;

    [Header("VFX")]
    [SerializeField] private ParticleSystem chargingParticles;
    [SerializeField] private ParticleSystem chargedParticles;

    [Header("Sequence")]
    [SerializeField] private RunePathSequence pathSequence;

    private float   chargeProgress = 0f;
    private bool    playerIsOn     = false;
    private bool    hasTriggered   = false;

    void Awake()
    {
        if (runeRenderer == null)
            runeRenderer = GetComponent<SpriteRenderer>();

        if (runeRenderer != null)
            runeRenderer.color = idleColor;

        if (chargeRingImage != null)
        {
            chargeRingImage.fillAmount = 0f;
            chargeRingImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (hasTriggered) return;

        if (playerIsOn)
        {
            chargeProgress += Time.deltaTime / chargeDuration;
            chargeProgress  = Mathf.Clamp01(chargeProgress);

            UpdateVisuals();

            if (chargeProgress >= 1f)
                OnFullyCharged();
        }
        else if (resetOnLeave && chargeProgress > 0f)
        {
            // Drain charge when player leaves
            chargeProgress -= Time.deltaTime / (chargeDuration * 0.5f);
            chargeProgress  = Mathf.Max(0f, chargeProgress);

            UpdateVisuals();
        }
    }

    // ─── Trigger ──────────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        playerIsOn = true;

        if (chargingParticles != null && !chargingParticles.isPlaying)
            chargingParticles.Play();

        if (chargeRingImage != null)
            chargeRingImage.gameObject.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerIsOn = false;

        if (chargingParticles != null)
            chargingParticles.Stop();
    }

    // ─── Charge Complete ──────────────────────────────────────────────────────────

    void OnFullyCharged()
    {
        hasTriggered = true;
        playerIsOn   = false;

        if (runeRenderer != null)
            runeRenderer.color = chargedColor;

        if (chargingParticles != null)
            chargingParticles.Stop();

        if (chargedParticles != null)
            chargedParticles.Play();

        if (chargeRingImage != null)
        {
            chargeRingImage.fillAmount = 1f;
            chargeRingImage.gameObject.SetActive(false);
        }

        Debug.Log("Trigger rune fully charged — starting sequence!");

        if (pathSequence != null)
            pathSequence.StartSequence();
        else
            Debug.LogWarning("TriggerRune: No RunePathSequence assigned!");
    }

    // ─── Visuals ──────────────────────────────────────────────────────────────────

    void UpdateVisuals()
    {
        // Lerp color from idle to charging based on progress
        if (runeRenderer != null)
            runeRenderer.color = Color.Lerp(idleColor, chargingColor, chargeProgress);

        // Update charge ring fill
        if (chargeRingImage != null)
            chargeRingImage.fillAmount = chargeProgress;
    }

    // ─── Public ───────────────────────────────────────────────────────────────────

    public float GetChargeProgress() => chargeProgress;
    public bool  HasTriggered()      => hasTriggered;

    public void Reset()
    {
        hasTriggered   = false;
        chargeProgress = 0f;
        playerIsOn     = false;

        if (runeRenderer != null)
            runeRenderer.color = idleColor;

        if (chargeRingImage != null)
        {
            chargeRingImage.fillAmount = 0f;
            chargeRingImage.gameObject.SetActive(false);
        }
    }
}