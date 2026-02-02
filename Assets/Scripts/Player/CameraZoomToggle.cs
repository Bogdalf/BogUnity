using UnityEngine;

/// <summary>
/// Two-state camera zoom system. Scroll wheel toggles between normal and zoomed-in view.
/// Attach to the Main Camera.
/// </summary>
public class CameraZoomToggle : MonoBehaviour
{
    [Header("Zoom Settings")]
    [SerializeField] private float normalSize = 5f; // Default camera size
    [SerializeField] private float zoomedSize = 3f; // Zoomed-in camera size
    [SerializeField] private float zoomSpeed = 5f; // How fast to transition

    [Header("Input")]
    [SerializeField] private float scrollThreshold = 0.1f; // Minimum scroll to trigger

    private Camera cam;
    private bool isZoomed = false;
    private float targetSize;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            Debug.LogError("CameraZoomToggle: No Camera component found!");
            enabled = false;
            return;
        }

        // Set initial size (may need to retry if camera just loaded)
        if (cam.orthographic)
        {
            cam.orthographicSize = normalSize;
            targetSize = normalSize;
        }
    }

    void Update()
    {
        // Retry finding camera component if somehow null
        if (cam == null)
        {
            cam = GetComponent<Camera>();
            if (cam == null) return;

            // Initialize on first successful find
            cam.orthographicSize = normalSize;
            targetSize = normalSize;
            Debug.Log("CameraZoomToggle: Found camera!");
        }

        HandleZoomInput();
        UpdateZoom();
    }

    void HandleZoomInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Scroll up = zoom in (to close-up state)
        if (scroll >= scrollThreshold && !isZoomed)
        {
            ZoomIn();
        }
        // Scroll down = zoom out (to normal state)
        else if (scroll <= -scrollThreshold && isZoomed)
        {
            ZoomOut();
        }
    }

    void ZoomIn()
    {
        isZoomed = true;
        targetSize = zoomedSize;
        Debug.Log("Zoomed in to close-up");
    }

    void ZoomOut()
    {
        isZoomed = false;
        targetSize = normalSize;
        Debug.Log("Zoomed out to normal");
    }

    void UpdateZoom()
    {
        // Smoothly lerp to target size
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetSize, Time.deltaTime * zoomSpeed);
    }

    /// <summary>
    /// Force zoom to a specific state (useful for cutscenes, etc.)
    /// </summary>
    public void SetZoomState(bool zoomed)
    {
        isZoomed = zoomed;
        targetSize = zoomed ? zoomedSize : normalSize;
    }

    /// <summary>
    /// Instantly snap to target zoom (no smooth transition)
    /// </summary>
    public void SnapToZoom(bool zoomed)
    {
        isZoomed = zoomed;
        targetSize = zoomed ? zoomedSize : normalSize;
        cam.orthographicSize = targetSize;
    }

    // Show zoom states in Scene view
    void OnDrawGizmosSelected()
    {
        Camera currentCam = GetComponent<Camera>();
        if (currentCam == null || !currentCam.orthographic) return;

        // Draw normal view bounds
        Gizmos.color = Color.green;
        float normalHeight = normalSize * 2f;
        float normalWidth = normalHeight * currentCam.aspect;
        Gizmos.DrawWireCube(transform.position, new Vector3(normalWidth, normalHeight, 0));

        // Draw zoomed view bounds
        Gizmos.color = Color.yellow;
        float zoomedHeight = zoomedSize * 2f;
        float zoomedWidth = zoomedHeight * currentCam.aspect;
        Gizmos.DrawWireCube(transform.position, new Vector3(zoomedWidth, zoomedHeight, 0));
    }
}