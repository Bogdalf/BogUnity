using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages spawning and tracking of spirits during an intermission wave.
/// Spawns 2 spirits at a time from a center point, each assigned to a statue.
/// Fires OnWaveComplete when all 11 spirits are resolved.
///
/// Setup:
///   - Assign spiritPrefab
///   - Assign all 11 SpiritStatue targets in order
///   - Set spawnCenter to the center of the arena circle
/// </summary>
public class SpiritWave : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject  spiritPrefab;
    [SerializeField] private Transform   spawnCenter;
    [SerializeField] private float       spawnRadius       = 0.5f; // Slight offset from exact center
    [SerializeField] private float       delayBetweenPairs = 3f;   // Seconds between each pair spawning

    [Header("Spirit Targets")]
    [Tooltip("All 11 statues in the order spirits should be assigned to them.")]
    [SerializeField] private List<SpiritStatue> statueTargets = new List<SpiritStatue>();

    public System.Action OnWaveComplete;

    private int  totalSpirits     = 0;
    private int  resolvedCount    = 0;
    private bool waveActive       = false;
    private bool waveComplete     = false;

    // ─── Public API ───────────────────────────────────────────────────────────────

    public void StartWave()
    {
        if (waveActive || waveComplete) return;
        if (statueTargets == null || statueTargets.Count == 0)
        {
            Debug.LogWarning("SpiritWave: No statue targets assigned!");
            return;
        }

        totalSpirits  = statueTargets.Count; // 11
        resolvedCount = 0;
        waveActive    = true;

        Debug.Log($"Spirit wave started — {totalSpirits} spirits total.");
        StartCoroutine(SpawnSequence());
    }

    public bool IsComplete() => waveComplete;
    public bool IsActive()   => waveActive;

    // ─── Spawn Sequence ───────────────────────────────────────────────────────────

    IEnumerator SpawnSequence()
    {
        int statueIndex = 0;

        while (statueIndex < totalSpirits)
        {
            // Spawn up to 2 spirits per wave
            int toSpawn = Mathf.Min(2, totalSpirits - statueIndex);

            for (int i = 0; i < toSpawn; i++)
            {
                if (statueIndex < statueTargets.Count)
                {
                    SpawnSpirit(statueTargets[statueIndex]);
                    statueIndex++;
                }
            }

            Debug.Log($"Spawned pair. Total spawned: {statueIndex}/{totalSpirits}");

            // Wait before spawning next pair, unless this was the last pair
            if (statueIndex < totalSpirits)
                yield return new WaitForSeconds(delayBetweenPairs);
        }
    }

    void SpawnSpirit(SpiritStatue target)
    {
        if (spiritPrefab == null || spawnCenter == null) return;

        // Slight random offset so they don't overlap exactly
        Vector2 offset      = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos    = spawnCenter.position + new Vector3(offset.x, offset.y, 0f);
        GameObject spiritGO = Instantiate(spiritPrefab, spawnPos, Quaternion.identity);

        SpiritAI spirit = spiritGO.GetComponent<SpiritAI>();
        if (spirit == null)
        {
            Debug.LogWarning("SpiritWave: Spirit prefab has no SpiritAI component!");
            return;
        }

        spirit.SetTarget(target);
        spirit.OnResolved += OnSpiritResolved;
    }

    // ─── Resolution ───────────────────────────────────────────────────────────────

    void OnSpiritResolved(SpiritAI spirit)
    {
        spirit.OnResolved -= OnSpiritResolved;
        resolvedCount++;

        Debug.Log($"Spirit resolved. {resolvedCount}/{totalSpirits}");

        if (resolvedCount >= totalSpirits)
            CompleteWave();
    }

    void CompleteWave()
    {
        if (waveComplete) return;

        waveActive    = false;
        waveComplete  = true;

        Debug.Log("Spirit wave complete!");
        OnWaveComplete?.Invoke();
    }
}