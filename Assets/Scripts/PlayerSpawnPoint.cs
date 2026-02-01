using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Place this in each scene to mark where the player should spawn.
/// When the scene loads, it will move the persistent player to this position.
/// </summary>
public class PlayerSpawnPoint : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private bool setPositionOnSceneLoad = true;
    [SerializeField] private bool resetVelocity = true;

    [Header("Visual Helper (Editor Only)")]
    [SerializeField] private Color gizmoColor = Color.green;
    [SerializeField] private float gizmoSize = 1f;

    void Start()
    {
        if (setPositionOnSceneLoad)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        // Find the persistent player
        if (PersistentPlayer.Instance != null)
        {
            // Move player to spawn point
            PersistentPlayer.Instance.transform.position = transform.position;
            PersistentPlayer.Instance.transform.rotation = transform.rotation;

            // Reset velocity if player has Rigidbody2D
            if (resetVelocity)
            {
                Rigidbody2D rb = PersistentPlayer.Instance.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
            }

            Debug.Log($"Player spawned at: {transform.position}");
        }
        else
        {
            Debug.LogWarning("PlayerSpawnPoint: PersistentPlayer.Instance not found! Make sure GameBootstrap has loaded first.");
        }
    }

    // Draw a visual indicator in the Scene view
    void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;

        // Draw a circle to show spawn location
        Gizmos.DrawWireSphere(transform.position, gizmoSize);

        // Draw a small arrow to show spawn direction
        Vector3 forward = transform.up * gizmoSize;
        Gizmos.DrawRay(transform.position, forward);
        Gizmos.DrawRay(transform.position + forward, Quaternion.Euler(0, 0, 135) * forward * 0.3f);
        Gizmos.DrawRay(transform.position + forward, Quaternion.Euler(0, 0, -135) * forward * 0.3f);
    }

    void OnDrawGizmosSelected()
    {
        // Draw a label when selected
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gizmoSize * 1.2f);
    }
}