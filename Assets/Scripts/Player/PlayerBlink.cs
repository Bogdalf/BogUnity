using System.Collections;
using UnityEngine;

public class PlayerBlink : MonoBehaviour
{
    [Header("Blink Settings")]
    [SerializeField] private float blinkDistance = 5f;
    [SerializeField] private float blinkCooldown = 1f;

    [Header("Timing")]
    [SerializeField] private float windupDuration = 0.4f;    // Time before teleport (match to your animation)
    [SerializeField] private float recoveryDuration = 0.2f;  // Time after teleport before control returns

    [Header("Invulnerability")]
    [SerializeField] private bool invulnerableDuringBlink = true;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool isBlinking = false;
    private float lastBlinkTime = -999f;
    private Vector2 blinkDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (InputManager.Instance != null && InputManager.Instance.IsPlayerInputBlocked())
            return;

        if (Input.GetKeyDown(KeyCode.Space) && CanBlink())
        {
            // Determine direction from movement input, fall back to mouse direction
            Vector2 moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );

            if (moveInput.magnitude > 0.1f)
            {
                blinkDirection = moveInput.normalized;
            }
            else
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                blinkDirection = (mousePos - transform.position).normalized;
            }

            StartCoroutine(BlinkCoroutine());
        }
    }

    bool CanBlink()
    {
        return !isBlinking && Time.time >= lastBlinkTime + blinkCooldown;
    }

    IEnumerator BlinkCoroutine()
    {
        isBlinking = true;
        lastBlinkTime = Time.time;

        // Freeze movement
        if (playerMovement != null)
            playerMovement.enabled = false;

        rb.linearVelocity = Vector2.zero;

        // Play blink animation
        if (animator != null)
            animator.SetTrigger("Blink");

        // Visual: fade out slightly during windup
        if (spriteRenderer != null)
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.4f);

        // Wait for windup — character stands still while animation plays
        yield return new WaitForSeconds(windupDuration);

        // TELEPORT
        transform.position += (Vector3)(blinkDirection * blinkDistance);

        // Visual: pop back to full opacity on arrival
        if (spriteRenderer != null)
            spriteRenderer.color = Color.white;

        // Brief recovery before control returns
        yield return new WaitForSeconds(recoveryDuration);

        // Restore movement
        if (playerMovement != null)
            playerMovement.enabled = true;

        isBlinking = false;
    }

    // Called by PlayerHealth to check i-frames
    public bool IsBlinking()
    {
        return isBlinking && invulnerableDuringBlink;
    }
}