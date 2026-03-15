using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f; // Sprint is 1.5x normal speed

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
        // Check centralized input manager FIRST
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsPlayerInputBlocked())
        {
            if (!PersistentInputManager.Instance.IsForcedMovement())
            {
                // Truly blocked — zero everything out
                if (animator != null) animator.SetFloat("Speed", 0);
                moveInput = Vector2.zero;
            }
            // If forced movement, let KingRecruitSequence drive the animator directly
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

        // Check if sprinting (holding Shift)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        // Move the player (physics-based)
        rb.linearVelocity = moveInput.normalized * currentSpeed;
    }
}