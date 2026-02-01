using UnityEngine;

/// <summary>
/// Controls 8-directional sprite animations based on mouse position.
/// Does NOT rotate the GameObject - just updates animator parameters.
/// </summary>
public class PlayerDirectionalSprite : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool use8Directions = true; // False for smooth blending
    [SerializeField] private float directionSmoothTime = 0.1f; // How fast direction changes (0 = instant)

    private Camera mainCamera;
    private Animator animator;

    // For smooth direction changes
    private Vector2 currentDirection;
    private Vector2 directionVelocity;

    // Animator parameter names
    private static readonly int DirectionXParam = Animator.StringToHash("DirectionX");
    private static readonly int DirectionYParam = Animator.StringToHash("DirectionY");

    void Start()
    {
        mainCamera = Camera.main;
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning("PlayerDirectionalSprite: No Animator found!");
        }
    }

    void Update()
    {
        // Retry finding camera/animator if null (for persistent player loading into new scenes)
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                Debug.Log("PlayerDirectionalSprite: Found camera!");
            }
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // If still null, skip this frame
        if (mainCamera == null || animator == null) return;

        UpdateDirection();
    }

    void UpdateDirection()
    {
        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate direction from player to mouse
        Vector2 targetDirection = (mousePosition - transform.position).normalized;

        if (use8Directions)
        {
            // Snap to 8 directions
            targetDirection = SnapTo8Directions(targetDirection);
        }

        // Smooth the direction change if smooth time > 0
        if (directionSmoothTime > 0)
        {
            currentDirection = Vector2.SmoothDamp(currentDirection, targetDirection, ref directionVelocity, directionSmoothTime);
            currentDirection.Normalize();
        }
        else
        {
            currentDirection = targetDirection;
        }

        // Update animator
        animator.SetFloat(DirectionXParam, currentDirection.x);
        animator.SetFloat(DirectionYParam, currentDirection.y);
    }

    /// <summary>
    /// Snaps a direction vector to one of 8 cardinal directions
    /// </summary>
    Vector2 SnapTo8Directions(Vector2 direction)
    {
        // Calculate angle in degrees
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Normalize to 0-360
        if (angle < 0) angle += 360;

        // Snap to nearest 45-degree increment
        // Each direction has a 45-degree range (22.5 degrees on each side)
        if (angle >= 337.5f || angle < 22.5f)
            return Vector2.right;        // East (0°)
        else if (angle >= 22.5f && angle < 67.5f)
            return new Vector2(0.707f, 0.707f);   // NorthEast (45°)
        else if (angle >= 67.5f && angle < 112.5f)
            return Vector2.up;           // North (90°)
        else if (angle >= 112.5f && angle < 157.5f)
            return new Vector2(-0.707f, 0.707f);  // NorthWest (135°)
        else if (angle >= 157.5f && angle < 202.5f)
            return Vector2.left;         // West (180°)
        else if (angle >= 202.5f && angle < 247.5f)
            return new Vector2(-0.707f, -0.707f); // SouthWest (225°)
        else if (angle >= 247.5f && angle < 292.5f)
            return Vector2.down;         // South (270°)
        else // 292.5f to 337.5f
            return new Vector2(0.707f, -0.707f);  // SouthEast (315°)
    }

    /// <summary>
    /// Manually set the facing direction (useful for cutscenes, etc.)
    /// </summary>
    public void SetDirection(Vector2 direction)
    {
        if (animator == null) return;

        direction.Normalize();

        if (use8Directions)
        {
            direction = SnapTo8Directions(direction);
        }

        animator.SetFloat(DirectionXParam, direction.x);
        animator.SetFloat(DirectionYParam, direction.y);
    }

    /// <summary>
    /// Get the current facing direction
    /// </summary>
    public Vector2 GetFacingDirection()
    {
        if (animator == null) return Vector2.down;

        return new Vector2(
            animator.GetFloat(DirectionXParam),
            animator.GetFloat(DirectionYParam)
        );
    }
}