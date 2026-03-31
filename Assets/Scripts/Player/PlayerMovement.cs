using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.5f;

    private Animator animator;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsPlayerInputBlocked())
        {
            if (!PersistentInputManager.Instance.IsForcedMovement())
            {
                if (animator != null) animator.SetFloat("Speed", 0);
                moveInput = Vector2.zero;
            }
            return;
        }

        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (animator != null)
        {
            float speed = moveInput.magnitude;
            animator.SetFloat("Speed", speed);

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
        float currentSpeed = moveSpeed;

        bool isSprinting = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (isSprinting)
        {
            currentSpeed *= sprintMultiplier;
        }

        rb.linearVelocity = moveInput.normalized * currentSpeed;
    }
}