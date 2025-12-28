using UnityEngine;

public class PlayerGathering : MonoBehaviour
{
    [Header("Gathering Settings")]
    [SerializeField] private float gatherRange = 1.5f;
    [SerializeField] private float gatherArc = 90f; // Degrees of arc
    [SerializeField] private float gatherCooldown = 0.8f;
    [SerializeField] private KeyCode gatherKey = KeyCode.F;

    [Header("Visual")]
    [SerializeField] private GameObject gatherVisualObject; // Could be same slash or a different pickaxe/axe swing
    [SerializeField] private float swingDuration = 0.2f;

    private float lastGatherTime = -999f;
    private bool isGathering = false;

    void Update()
    {
        // Check centralized input manager FIRST
        if (InputManager.Instance != null && InputManager.Instance.IsPlayerInputBlocked())
        {
            return;
        }

        // F key to gather
        if (Input.GetKeyDown(gatherKey) && CanGather())
        {
            Gather();
        }
    }

    bool CanGather()
    {
        return !isGathering && Time.time >= lastGatherTime + gatherCooldown;
    }

    void Gather()
    {
        isGathering = true;
        lastGatherTime = Time.time;

        // Get direction player is facing (toward mouse)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 gatherDirection = (mousePos - transform.position).normalized;

        // Find all colliders in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, gatherRange);

        Debug.Log("Gathering - found " + hits.Length + " colliders in range");

        foreach (Collider2D hit in hits)
        {
            // Check if it's a gatherable object
            GatherableObject gatherable = hit.GetComponent<GatherableObject>();

            if (gatherable != null && gatherable.CanBeGathered())
            {
                // Check if gatherable is within gather arc
                Vector3 directionToGatherable = (hit.transform.position - transform.position).normalized;
                float angleToGatherable = Vector3.Angle(gatherDirection, directionToGatherable);

                Debug.Log("Gatherable angle: " + angleToGatherable + " degrees (arc is " + (gatherArc / 2f) + ")");

                if (angleToGatherable <= gatherArc / 2f)
                {
                    Debug.Log("GATHERABLE IN ARC - GATHERING!");
                    gatherable.OnGathered(gameObject);
                }
            }
        }

        // Visual feedback
        StartCoroutine(SwingVisual());
    }

    System.Collections.IEnumerator SwingVisual()
    {
        if (gatherVisualObject != null)
        {
            // Orient swing toward mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gatherVisualObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            // Show it
            gatherVisualObject.SetActive(true);

            // Wait
            yield return new WaitForSeconds(swingDuration);

            // Hide it
            gatherVisualObject.SetActive(false);
        }

        isGathering = false;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, gatherRange);
    }

    public float GetCooldownPercent()
    {
        if (isGathering)
        {
            return 1f;
        }

        float timeSinceLastGather = Time.time - lastGatherTime;

        if (timeSinceLastGather >= gatherCooldown)
        {
            return 0f;
        }
        else
        {
            return 1f - (timeSinceLastGather / gatherCooldown);
        }
    }

    public bool IsGathering()
    {
        return isGathering;
    }
}