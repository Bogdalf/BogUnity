using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerCharge playerCharge;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCharge = GetComponent<PlayerCharge>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PersistentInputManager.Instance == null)
        {
            Debug.LogWarning("PersistentInputManager.Instance is NULL!");
        }

        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsPlayerInputBlocked())
        {
            Debug.Log("Input is BLOCKED");
            // Don't accept input, but keep animator updated with zero speed
            if (animator != null)
            {
                animator.SetFloat("Speed", 0);
            }
            moveInput = Vector2.zero;
            return;
        }

        // Get input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        // Update animator with movement direction
        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);

            // Normalize for direction (if moving)
            if (speed > 0.1f)
            {
                Vector2 normalized = moveInput.normalized;
                animator.SetFloat("MovementX", normalized.x);
                animator.SetFloat("MovementY", normalized.y);
            }
        }
    }

    void FixedUpdate()
    {
        // Calculate movement speed (slow down if aiming charge)
        float currentSpeed = moveSpeed;

        if (playerCharge != null && playerCharge.IsAiming())
        {
            currentSpeed *= playerCharge.GetAimingMovementMultiplier();
        }

        // Move the player (physics-based)
        rb.linearVelocity = moveInput.normalized * currentSpeed;
    }
}