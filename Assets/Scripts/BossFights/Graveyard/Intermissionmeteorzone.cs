using System.Collections;
using UnityEngine;

/// <summary>
/// Continuously spawns meteors within a circular zone during spirit waves.
/// Acts as a passive hazard to pressure the player.
/// 
/// Enable/disable via BossIntermissionManager.
/// </summary>
public class IntermissionMeteorZone : MonoBehaviour
{
    [Header("Zone")]
    [SerializeField] private float zoneRadius = 8f;

    [Header("Meteor Settings")]
    [SerializeField] private GameObject meteorPrefab;
    [SerializeField] private float      minInterval = 1.5f;
    [SerializeField] private float      maxInterval = 3f;
    [SerializeField] private bool       targetPlayer = true; // If true, biases toward player position

    [Header("Target Bias")]
    [Tooltip("0 = fully random in zone, 1 = always on player")]
    [SerializeField] private float playerBias = 0.6f;

    private bool   isActive = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // ─── Public API ───────────────────────────────────────────────────────────────

    public void StartZone()
    {
        if (isActive) return;
        isActive = true;
        StartCoroutine(MeteorLoop());
        Debug.Log("Intermission meteor zone started.");
    }

    public void StopZone()
    {
        isActive = false;
        StopAllCoroutines();
        Debug.Log("Intermission meteor zone stopped.");
    }

    // ─── Meteor Loop ──────────────────────────────────────────────────────────────

    IEnumerator MeteorLoop()
    {
        while (isActive)
        {
            float interval = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(interval);

            if (!isActive) break;

            SpawnMeteor();
        }
    }

    void SpawnMeteor()
    {
        if (meteorPrefab == null) return;

        Vector2 target = GetTargetPosition();
        MeteorProjectile.Spawn(meteorPrefab, target);
    }

    Vector2 GetTargetPosition()
    {
        // Random position within zone
        Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * zoneRadius;

        if (targetPlayer && player != null)
        {
            // Blend between random and player position based on bias
            Vector2 playerPos = player.position;
            return Vector2.Lerp(randomPos, playerPos, playerBias);
        }

        return randomPos;
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, zoneRadius);
    }
}