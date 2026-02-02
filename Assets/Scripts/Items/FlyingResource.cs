using UnityEngine;
using System.Collections;

/// <summary>
/// Visual feedback for gathered resources - flies to player then disappears.
/// Spawned by GatherableObject when resources are collected.
/// </summary>
public class FlyingResource : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float initialBurstSpeed = 3f;
    [SerializeField] private float burstDuration = 0.2f;
    [SerializeField] private float flySpeed = 8f;
    [SerializeField] private float arrivalDistance = 0.5f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Transform target;
    private Vector2 burstDirection;
    private bool isBursting = true;
    private float burstTimer = 0f;

    public void Initialize(Sprite resourceSprite, Transform playerTarget, Vector2 initialDirection)
    {
        if (spriteRenderer != null && resourceSprite != null)
        {
            spriteRenderer.sprite = resourceSprite;
        }

        target = playerTarget;
        burstDirection = initialDirection.normalized;

        StartCoroutine(FlyToPlayer());
    }

    IEnumerator FlyToPlayer()
    {
        // Phase 1: Burst outward
        while (burstTimer < burstDuration)
        {
            burstTimer += Time.deltaTime;
            transform.position += (Vector3)burstDirection * initialBurstSpeed * Time.deltaTime;
            yield return null;
        }

        isBursting = false;

        // Phase 2: Fly to player
        while (target != null)
        {
            // Move toward player
            Vector3 direction = (target.position - transform.position).normalized;
            transform.position += direction * flySpeed * Time.deltaTime;

            // Increase speed over time (acceleration)
            flySpeed += 5f * Time.deltaTime;

            // Check if arrived
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < arrivalDistance)
            {
                // Arrived! Destroy self
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }

        // If target destroyed somehow, clean up
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        // Could trigger particle effect or sound here
    }
}