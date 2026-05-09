using UnityEngine;

/// <summary>
/// A statue that spirits are walking toward.
/// When a spirit reaches it, the statue becomes empowered and
/// adds a 50% health bonus to Boss2.
///
/// Place 11 of these in the scene, one per spirit.
/// </summary>
public class SpiritStatue : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer statueRenderer;
    [SerializeField] private Color defaultColor   = Color.white;
    [SerializeField] private Color empoweredColor = new Color(1f, 0.4f, 0.1f, 1f);

    [Header("VFX")]
    [SerializeField] private ParticleSystem empowerParticles;

    // Static tracker — persists across all statue instances
    // Resets when the intermission begins
    public static int EmpoweredCount { get; private set; } = 0;
    public static float TotalHealthBonus => EmpoweredCount * 0.5f; // 50% per statue

    public bool IsEmpowered { get; private set; } = false;

    void Awake()
    {
        if (statueRenderer == null)
            statueRenderer = GetComponent<SpriteRenderer>();

        if (statueRenderer != null)
            statueRenderer.color = defaultColor;
    }

    // ─── Static Reset ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Call this at the start of the intermission to reset the counter.
    /// </summary>
    public static void ResetAll()
    {
        EmpoweredCount = 0;
        Debug.Log("Spirit statue count reset.");
    }

    // ─── Empowerment ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by SpiritAI when it reaches this statue.
    /// </summary>
    public void Empower()
    {
        if (IsEmpowered) return;

        IsEmpowered = true;
        EmpoweredCount++;

        if (statueRenderer != null)
            statueRenderer.color = empoweredColor;

        if (empowerParticles != null)
            empowerParticles.Play();

        Debug.Log($"Statue empowered! Total: {EmpoweredCount} statues, Boss2 health bonus: +{TotalHealthBonus * 100f}%");
    }
}