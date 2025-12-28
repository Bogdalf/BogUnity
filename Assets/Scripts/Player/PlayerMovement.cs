using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerCharge playerCharge;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCharge = GetComponent<PlayerCharge>();
    }

    void Update()
    {
        // Check centralized input manager
        if (InputManager.Instance != null && InputManager.Instance.IsPlayerInputBlocked())
        {
            moveInput = Vector2.zero;
            return;
        }

        // Get input every frame
        moveInput.x = Input.GetAxisRaw("Horizontal"); // A/D or Arrow keys
        moveInput.y = Input.GetAxisRaw("Vertical");   // W/S or Arrow keys
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