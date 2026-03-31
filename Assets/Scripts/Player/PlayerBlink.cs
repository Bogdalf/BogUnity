using System.Collections;
using UnityEngine;

public class PlayerBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkDistance = 5f;
    [SerializeField] private float blinkCooldown = 1f;

    [Header("Timing")]
    [SerializeField] private float windupDuration = 0.4f;
    [SerializeField] private float recoveryDuration = 0.2f;

    [Header("Collision")]
    [SerializeField] private LayerMask collisionMask; // Set to your walls/buildings layers in the Inspector
    [SerializeField] private float castRadius = 0.25f; // Should match ~half your collider's effective world size
    [SerializeField] private float skinBuffer = 0.1f;  // How far to stay from the wall surface on landing

    [Header("Invulnerability")]
    [SerializeField] private bool invulnerableDuringBlink = true;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isBlinking = false;
    private float lastBlinkTime = -999f;
    private Vector2 blinkDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsPlayerInputBlocked())
            return;

        if (Input.GetKeyDown(KeyCode.Space) && CanBlink())
        {
            Vector2 moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            blinkDirection = moveInput.magnitude > 0.1f
                ? moveInput.normalized
                : (Vector2)(Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;

            StartCoroutine(BlinkCoroutine());
        }
    }

    bool CanBlink()
    {
        return !isBlinking && Time.time >= lastBlinkTime + blinkCooldown;
    }

    /// <summary>
    /// Casts a circle along the blink path and returns the furthest
    /// reachable position, stopping just before any collider hit.
    /// </summary>
    Vector2 GetBlinkDestination()
    {
        Vector2 origin = transform.position;

        RaycastHit2D hit = Physics2D.CircleCast(
            origin,
            castRadius,
            blinkDirection,
            blinkDistance,
            collisionMask
        );

        if (hit.collider != null)
        {
            // Land just before the surface, pulled back by skinBuffer
            float safeDistance = Mathf.Max(0f, hit.distance - skinBuffer);
            return origin + blinkDirection * safeDistance;
        }

        // Nothing in the way — travel the full distance
        return origin + blinkDirection * blinkDistance;
    }

    IEnumerator BlinkCoroutine()
    {
        isBlinking = true;
        lastBlinkTime = Time.time;

        // Calculate destination before windup so it's based on
        // where the player aimed, not where they end up after animation
        Vector2 destination = GetBlinkDestination();

        if (playerMovement != null)
            playerMovement.enabled = false;

        rb.linearVelocity = Vector2.zero;

        if (animator != null)
            animator.SetTrigger("Blink");

        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);

        yield return new WaitForSeconds(windupDuration);

        transform.position = destination;

        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        yield return new WaitForSeconds(recoveryDuration);

        if (playerMovement != null)
            playerMovement.enabled = true;

        isBlinking = false;
    }

    public bool IsBlinking()
    {
        return isBlinking && invulnerableDuringBlink;
    }

    public float GetCooldownPercent()
    {
        if (isBlinking) return 1f;
        float timeSince = Time.time - lastBlinkTime;
        if (timeSince >= blinkCooldown) return 0f;
        return 1f - (timeSince / blinkCooldown);
    }
}