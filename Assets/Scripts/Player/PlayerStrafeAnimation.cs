using UnityEngine;

/// <summary>
/// Calculates strafe direction for animations based on movement input vs facing direction.
/// Works with PlayerDirectionalSprite and PlayerMovement.
/// </summary>
public class PlayerStrafeAnimation : MonoBehaviour
{
    private Animator animator;
    private PlayerDirectionalSprite directionalSprite;

    private static readonly int StrafeDirectionParam = Animator.StringToHash("StrafeDirection");

    void Start()
    {
        animator = GetComponent<Animator>();
        directionalSprite = GetComponent<PlayerDirectionalSprite>();

        if (animator == null)
        {
            Debug.LogWarning("PlayerStrafeAnimation: No Animator found!");
        }

        if (directionalSprite == null)
        {
            Debug.LogWarning("PlayerStrafeAnimation: No PlayerDirectionalSprite found!");
        }
    }

    void Update()
    {
        if (animator == null || directionalSprite == null) return;

        CalculateStrafeDirection();
    }

    void CalculateStrafeDirection()
    {
        // Get movement input (WASD)
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        // If not moving, no strafe
        if (moveInput.magnitude < 0.1f)
        {
            animator.SetFloat(StrafeDirectionParam, 0f);
            return;
        }

        // Normalize movement direction
        Vector2 moveDirection = moveInput.normalized;

        // Get facing direction (where mouse is pointing)
        Vector2 facingDirection = directionalSprite.GetFacingDirection();

        // Calculate angle between facing and movement
        float angle = Vector2.SignedAngle(facingDirection, moveDirection);

        // SNAP to discrete animations (no blending between them)
        float strafeValue = 0f;

        if (angle >= -69f && angle < 69f)
        {
            // Moving forward (within 45° of facing)
            strafeValue = 0f;
        }
        else if (angle >= 69f && angle < 111f)
        {
            // Strafing right (45° to 135°)
            strafeValue = 1f;
        }
        else if (angle >= -111f && angle < -69f)
        {
            // Strafing left (-135° to -45°)
            strafeValue = -1f;
        }
        else // angle >= 135° or angle < -135°
        {
            // Moving backward (more than 135° from facing)
            strafeValue = 2f; // Or use a separate backward value if you have that
        }

        animator.SetFloat(StrafeDirectionParam, strafeValue);

        // Debug visualization
        // Debug.Log($"Move angle: {angle:F1}° → Strafe: {strafeValue:F2}");
    }

    /// <summary>
    /// Get the relative movement direction for more complex animation systems
    /// </summary>
    public Vector2 GetRelativeMovement()
    {
        Vector2 moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );

        if (moveInput.magnitude < 0.1f) return Vector2.zero;

        Vector2 moveDirection = moveInput.normalized;
        Vector2 facingDirection = directionalSprite.GetFacingDirection();

        // Convert to local space (relative to facing direction)
        float angle = Mathf.Atan2(facingDirection.y, facingDirection.x);
        float cos = Mathf.Cos(-angle);
        float sin = Mathf.Sin(-angle);

        Vector2 localMovement = new Vector2(
            moveDirection.x * cos - moveDirection.y * sin,
            moveDirection.x * sin + moveDirection.y * cos
        );

        return localMovement;
    }
}