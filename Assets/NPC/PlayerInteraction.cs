using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private LayerMask npcLayer;

    private NPC currentNPC = null;

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
        // Find all colliders in interaction range
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, interactionRange);

        NPC closestNPC = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            NPC npc = collider.GetComponent<NPC>();
            if (npc != null)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNPC = npc;
                }
            }
        }

        currentNPC = closestNPC;
    }

    void OnDrawGizmosSelected()
    {
        // Draw interaction range in Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}