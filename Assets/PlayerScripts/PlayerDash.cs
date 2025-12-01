using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Invulnerability")]
    [SerializeField] private bool invulnerableDuringDash = true;

    private Collider2D playerCollider;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerHealth playerHealth;
    private TalentTreeUI talentTreeUI;
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float lastDashTime = -999f;
    private Vector2 dashDirection;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerHealth = GetComponent<PlayerHealth>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        talentTreeUI = FindObjectOfType<TalentTreeUI>();
    }

    void Update()
    {
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
        {
            return;
        }
        
        // Check for dash input        
        if (Input.GetKeyDown(KeyCode.Space) && CanDash())
        {
            StartDash();
        }

        // Handle dash timer
        if (isDashing)
        {
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0)
            {
                EndDash();
            }
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            // Override movement during dash
            rb.linearVelocity = dashDirection * dashSpeed;
        }
    }

    bool CanDash()
    {
        return !isDashing && Time.time >= lastDashTime + dashCooldown;
    }

    void StartDash()
    {
        // Get movement input for dash direction
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // If no input, dash in the direction player is facing (toward mouse)
        if (moveInput.magnitude < 0.1f)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dashDirection = (mousePos - transform.position).normalized;
        }
        else
        {
            dashDirection = moveInput.normalized;
        }

        isDashing = true;
        dashTimeLeft = dashDuration;
        lastDashTime = Time.time;

        // Disable normal movement
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Disable collision during dash
        if (playerCollider != null)
        {
            playerCollider.enabled = false; // Add this line
        }

        // Visual feedback - make player semi-transparent during dash
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
        }

        Debug.Log("DASH!");
    }

    void EndDash()
    {
        isDashing = false;

        // Re-enable normal movement
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        // Re-enable collision
        if (playerCollider != null)
        {
            playerCollider.enabled = true; // Add this line
        }

        // Restore normal appearance
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }

    public float GetCooldownPercent()
    {
        if (isDashing)
        {
            // During dash, show as "on cooldown"
            return 1f;
        }

        float timeSinceLastDash = Time.time - lastDashTime;

        if (timeSinceLastDash >= dashCooldown)
        {
            // Ready
            return 0f;
        }
        else
        {
            // Still on cooldown - return percent remaining
            return 1f - (timeSinceLastDash / dashCooldown);
        }
    }
}