using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Drives the TilemapReveal shader across multiple tilemap layers simultaneously.
/// Also controls the foreshock pulse and synchronized edge spark ring.
///
/// TriggerMode.Dialogue  — Call TriggerReveal() manually from a dialogue event (used in the intro scene).
/// TriggerMode.Proximity — Triggers automatically when the player enters the activation radius.
///
/// Designed to be dropped as a prefab into any scene.
/// </summary>
public class MoonPlaneReveal : MonoBehaviour
{
    public enum TriggerMode { Dialogue, Proximity }

    [Header("Trigger")]
    [SerializeField] private TriggerMode triggerMode = TriggerMode.Dialogue;
    [SerializeField] private float proximityRadius = 6f;
    [SerializeField] private string playerTag = "Player";

    [Header("Tilemap Layers")]
    [SerializeField] private List<Tilemap> revealLayers;

    [Header("Reveal Settings")]
    [SerializeField] private Transform originTransform;
    [SerializeField] private float revealDuration = 3f;
    [SerializeField] private float maxRadius = 20f;

    [Header("Particle Effects")]
    [SerializeField] private ParticleSystem foreshockPulse;
    [SerializeField] private ParticleSystem revealEdgeSparks;

    [Header("Spawner (optional)")]
    [SerializeField] private MoonRiftSpawner riftSpawner;

    [Header("Scene Sequence (optional)")]
    [SerializeField] private SunPriestessInterjection sunPriestessInterjection;

    // Cached materials
    private List<Material> revealMaterials = new List<Material>();
    private bool triggered = false;

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

        // Sparks start stopped — we'll play them when the reveal begins
        if (revealEdgeSparks != null)
        {
            revealEdgeSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetEdgeSparkRadius(0f);
        }
    }

    void Update()
    {
        if (triggered) return;

        if (triggerMode == TriggerMode.Proximity)
        {
            // Simple distance check — no trigger collider needed on the prefab
            GameObject player = GameObject.FindWithTag(playerTag);
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= proximityRadius)
                    TriggerReveal();
            }
        }
    }

    /// <summary>
    /// Call this from KhatunSequence (or any dialogue event) when TriggerMode is Dialogue.
    /// Safe to call from proximity path too — guarded by the triggered flag.
    /// </summary>
    public void TriggerReveal()
    {
        if (triggered) return;
        triggered = true;
        StartCoroutine(RevealSequence());
    }

    IEnumerator RevealSequence()
    {
        Vector3 origin3D = originTransform != null ? originTransform.position : transform.position;
        Vector2 center = (Vector2)origin3D;

        // Set center on all materials
        foreach (Material mat in revealMaterials)
            mat.SetVector(CenterProperty, center);

        // --- Fire foreshock pulse ---
        if (foreshockPulse != null)
        {
            foreshockPulse.transform.position = origin3D;
            foreshockPulse.Play();
        }

        // --- Kick off monster spawner in parallel with the reveal ---
        if (riftSpawner != null)
            riftSpawner.BeginSpawning();

        // --- Start the Sun Priestess countdown ---
        if (sunPriestessInterjection != null)
            sunPriestessInterjection.BeginCountdown();

        yield return new WaitForSeconds(0.3f);

        // --- Start edge sparks at radius 0, let them loop permanently ---
        if (revealEdgeSparks != null)
        {
            revealEdgeSparks.transform.position = origin3D;
            SetEdgeSparkRadius(0f);
            revealEdgeSparks.Play();
        }

        // --- Animate reveal radius ---
        float elapsed = 0f;

        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / revealDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float currentRadius = Mathf.Lerp(0f, maxRadius, smoothT);

            foreach (Material mat in revealMaterials)
                mat.SetFloat(RadiusProperty, currentRadius);

            SetEdgeSparkRadius(currentRadius);

            yield return null;
        }

        // Lock to final radius
        foreach (Material mat in revealMaterials)
            mat.SetFloat(RadiusProperty, maxRadius);

        SetEdgeSparkRadius(maxRadius);

        // Sparks stay playing at maxRadius permanently — the rift is now open.
        // No Stop() call here.

        Debug.Log("[MoonPlaneReveal] Rift fully open.");
    }

    void SetEdgeSparkRadius(float radius)
    {
        if (revealEdgeSparks == null) return;
        var shape = revealEdgeSparks.shape;
        shape.radius = radius;
    }

    public void ResetReveal()
    {
        StopAllCoroutines();
        triggered = false;

        foreach (Material mat in revealMaterials)
            mat.SetFloat(RadiusProperty, 0f);

        if (foreshockPulse != null)
            foreshockPulse.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (revealEdgeSparks != null)
        {
            revealEdgeSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            SetEdgeSparkRadius(0f);
        }
    }

    void OnDestroy()
    {
        foreach (Material mat in revealMaterials)
            Destroy(mat);
    }

    void OnDrawGizmosSelected()
    {
        if (triggerMode == TriggerMode.Proximity)
        {
            Gizmos.color = new Color(0.4f, 0.2f, 0.8f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, proximityRadius);
        }
    }
}