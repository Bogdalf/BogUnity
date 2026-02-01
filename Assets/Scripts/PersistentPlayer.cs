using UnityEngine;

/// <summary>
/// Marks the Player GameObject to persist across scene loads.
/// Attach this to the root Player GameObject.
/// </summary>
public class PersistentPlayer : MonoBehaviour
{
    public static PersistentPlayer Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern - only one player can exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("Player marked as persistent");
        }
        else
        {
            // Destroy duplicate players
            Debug.LogWarning("Duplicate player detected - destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this when transitioning to a new scene to reposition the player
    /// </summary>
    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /// <summary>
    /// Call this to reset player to a spawn point (e.g., on scene load)
    /// </summary>
    public void ResetToSpawnPoint(Transform spawnPoint)
    {
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
    }
}