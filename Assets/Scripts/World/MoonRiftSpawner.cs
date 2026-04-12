using UnityEngine;
using System.Collections;

/// <summary>
/// Spawns Light Amalgamations (or any enemy prefab) from an expanding circular area
/// that grows in sync with the MoonPlaneReveal effect.
///
/// Add this component to the same GameObject as MoonPlaneReveal.
/// Call BeginSpawning() from MoonPlaneReveal (or directly) once the rift opens.
///
/// The spawn area is a circle that interpolates from 0 to spawnRadius over
/// growthDuration seconds — matching the visual expand of the reveal.
/// After that it stays at full size while monsters spawn until the budget is exhausted.
/// </summary>
public class MoonRiftSpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Budget")]
    [Tooltip("How many enemies to spawn total before the spawner goes dormant.")]
    [SerializeField] private int totalSpawnCount = 10;

    [Tooltip("Seconds between each spawn.")]
    [SerializeField] private float spawnInterval = 1f;

    [Header("Spawn Area")]
    [Tooltip("Max radius enemies can appear within. Should roughly match MoonPlaneReveal's maxRadius.")]
    [SerializeField] private float spawnRadius = 8f;

    [Tooltip("How long the spawn area takes to grow to full size (match MoonPlaneReveal.revealDuration).")]
    [SerializeField] private float growthDuration = 3f;

    [Tooltip("Minimum distance from the rift center an enemy can spawn (keeps them off the exact origin).")]
    [SerializeField] private float minSpawnRadius = 1f;

    [Header("Spawn Timing")]
    [Tooltip("Delay after BeginSpawning() is called before the first enemy appears. " +
             "Useful for letting the rift open a beat before monsters pour out.")]
    [SerializeField] private float initialDelay = 1f;

    [Header("Debug")]
    [SerializeField] private bool showGizmos = true;

    private float currentSpawnRadius = 0f;
    private int spawned = 0;
    private bool spawning = false;

    /// <summary>
    /// Call this to kick off spawning. Designed to be called by MoonPlaneReveal
    /// at the moment TriggerReveal() fires, so the area grows in sync.
    /// </summary>
    public void BeginSpawning()
    {
        if (spawning) return;
        spawning = true;
        StartCoroutine(GrowSpawnArea());
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// Smoothly grows currentSpawnRadius from 0 to spawnRadius over growthDuration seconds.
    /// Runs in parallel with SpawnLoop so early spawns land near the center.
    /// </summary>
    IEnumerator GrowSpawnArea()
    {
        float elapsed = 0f;

        while (elapsed < growthDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / growthDuration);
            currentSpawnRadius = Mathf.Lerp(0f, spawnRadius, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        currentSpawnRadius = spawnRadius;
    }

    /// <summary>
    /// Waits for initialDelay, then spawns one enemy per spawnInterval until budget is exhausted.
    /// </summary>
    IEnumerator SpawnLoop()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("[MoonRiftSpawner] No enemy prefab assigned!");
            yield break;
        }

        yield return new WaitForSeconds(initialDelay);

        while (spawned < totalSpawnCount)
        {
            SpawnEnemy();
            spawned++;

            if (spawned < totalSpawnCount)
                yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log($"[MoonRiftSpawner] Spawn budget exhausted ({totalSpawnCount} enemies spawned).");
        spawning = false;
    }

    void SpawnEnemy()
    {
        // Pick a random point inside the current spawn circle,
        // clamped to minSpawnRadius so nothing spawns right on top of the origin
        float radius = Random.Range(
            Mathf.Min(minSpawnRadius, currentSpawnRadius),
            Mathf.Max(minSpawnRadius, currentSpawnRadius)
        );

        Vector2 direction = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = transform.position + (Vector3)(direction * radius);

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    public void ResetSpawner()
    {
        StopAllCoroutines();
        spawned = 0;
        spawning = false;
        currentSpawnRadius = 0f;
    }

    void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        // Full spawn radius at runtime — purple
        Gizmos.color = new Color(0.6f, 0.1f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);

        // Min spawn radius — red so you can see the dead zone
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, minSpawnRadius);

        // Live radius during play (only visible while running)
        if (Application.isPlaying && spawning)
        {
            Gizmos.color = new Color(0.8f, 0.4f, 1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, currentSpawnRadius);
        }
    }
}