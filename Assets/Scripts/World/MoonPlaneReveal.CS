using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Drives the TilemapReveal shader across multiple tilemap layers simultaneously.
/// Also controls the foreshock pulse and synchronized edge spark ring.
/// Attach to any GameObject in the boss arena scene.
/// Call TriggerReveal() from KhatunSequence to start the effect.
/// </summary>
public class MoonPlaneReveal : MonoBehaviour
{
    [Header("Tilemap Layers")]
    [SerializeField] private List<Tilemap> revealLayers;

    [Header("Reveal Settings")]
    [SerializeField] private Transform originTransform;
    [SerializeField] private float revealDuration = 3f;
    [SerializeField] private float maxRadius = 20f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem foreshockPulse;
    [SerializeField] private ParticleSystem revealEdgeSparks;

    // Cached materials
    private List<Material> revealMaterials = new List<Material>();

    private static readonly int RadiusProperty = Shader.PropertyToID("_Radius");
    private static readonly int CenterProperty = Shader.PropertyToID("_Center");

    void Start()
    {
        foreach (Tilemap tilemap in revealLayers)
        {
            TilemapRenderer tilemapRenderer = tilemap.GetComponent<TilemapRenderer>();
            if (tilemapRenderer == null) continue;

            Material instance = new Material(tilemapRenderer.material);
            tilemapRenderer.material = instance;
            revealMaterials.Add(instance);

            instance.SetFloat(RadiusProperty, 0f);
        }

        // Position both particle systems at the origin
        UpdateParticlePositions();
    }

    void UpdateParticlePositions()
    {
        Vector3 origin = originTransform != null ? originTransform.position : Vector3.zero;

        if (foreshockPulse != null)
            foreshockPulse.transform.position = origin;

        if (revealEdgeSparks != null)
            revealEdgeSparks.transform.position = origin;
    }

    public void TriggerReveal()
    {
        StartCoroutine(RevealSequence());
    }

    // Test trigger — remove when hooking up to real sequence
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            TriggerReveal();
    }

    IEnumerator RevealSequence()
    {
        Vector2 center = originTransform != null
            ? (Vector2)originTransform.position
            : Vector2.zero;

        // Set center on all materials
        foreach (Material mat in revealMaterials)
            mat.SetVector(CenterProperty, center);

        // --- Fire foreshock pulse immediately ---
        if (foreshockPulse != null)
        {
            foreshockPulse.transform.position = originTransform != null
                ? originTransform.position
                : Vector3.zero;
            foreshockPulse.Play();
        }

        // Brief pause after foreshock before main reveal starts
        // Gives the pulse time to shoot out before the circle begins
        yield return new WaitForSeconds(0.3f);

        // --- Start edge sparks ---
        if (revealEdgeSparks != null)
        {
            revealEdgeSparks.transform.position = originTransform != null
                ? originTransform.position
                : Vector3.zero;

            // Set initial shape radius to 0 before playing
            SetEdgeSparkRadius(0f);
            revealEdgeSparks.Play();
        }

        // --- Animate radius ---
        float elapsed = 0f;

        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / revealDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentRadius = Mathf.Lerp(0f, maxRadius, smoothT);

            // Update shader on all layers
            foreach (Material mat in revealMaterials)
                mat.SetFloat(RadiusProperty, currentRadius);

            // Keep edge sparks locked to the circle boundary
            SetEdgeSparkRadius(currentRadius);

            yield return null;
        }

        // Ensure we land exactly at max
        foreach (Material mat in revealMaterials)
            mat.SetFloat(RadiusProperty, maxRadius);

        SetEdgeSparkRadius(maxRadius);

        // Let the trailing sparks finish naturally
        if (revealEdgeSparks != null)
            revealEdgeSparks.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        Debug.Log("Moon plane reveal complete");
    }

    /// <summary>
    /// Updates the edge spark particle system's shape radius to match the current reveal radius.
    /// This is what keeps the sparks locked to the expanding circle boundary.
    /// </summary>
    void SetEdgeSparkRadius(float radius)
    {
        if (revealEdgeSparks == null) return;

        var shape = revealEdgeSparks.shape;
        shape.radius = radius;
    }

    public void ResetReveal()
    {
        StopAllCoroutines();

        foreach (Material mat in revealMaterials)
            mat.SetFloat(RadiusProperty, 0f);

        if (foreshockPulse != null) foreshockPulse.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (revealEdgeSparks != null) revealEdgeSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    void OnDestroy()
    {
        foreach (Material mat in revealMaterials)
            Destroy(mat);
    }
}