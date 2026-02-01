using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10);

    void Start()
    {
        // If target not set, find the persistent player
        if (target == null)
        {
            FindPlayer();
        }
    }

    void LateUpdate()
    {
        // If target still null, try to find it again
        if (target == null)
        {
            FindPlayer();
            return;
        }

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    void FindPlayer()
    {
        // Try to find via PersistentPlayer
        if (PersistentPlayer.Instance != null)
        {
            target = PersistentPlayer.Instance.transform;
            Debug.Log("Camera found persistent player");
        }
        else
        {
            // Fallback - find by tag
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                Debug.Log("Camera found player by tag");
            }
        }
    }

    // Public method to set target if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}