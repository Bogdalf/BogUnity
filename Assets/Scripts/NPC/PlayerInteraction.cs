using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask npcLayer;

    private IInteractable currentNPC = null;

    void Update()
    {
        // Check for nearby NPCs
        CheckForNearbyNPC();

        // Handle E key press for interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (currentNPC != null && currentNPC.IsPlayerNearby())
            {
                currentNPC.Interact();
            }
        }
    }

    void CheckForNearbyNPC()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);
        IInteractable closest = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = interactable;
                }
            }
        }
        currentNPC = closest;
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}