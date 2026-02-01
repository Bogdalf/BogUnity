using UnityEngine;

public class PlayerAiming : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        FindCamera();
    }

    void Update()
    {
        // If camera is null, try to find it
        if (mainCamera == null)
        {
            FindCamera();
            return; // Skip this frame if still null
        }

        // Get mouse position in world space
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate direction from player to mouse
        Vector3 direction = mousePosition - transform.position;

        // Calculate angle and rotate player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90); // -90 because sprites face up by default
    }

    void FindCamera()
    {
        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            // Fallback - find any camera with MainCamera tag
            GameObject cameraObj = GameObject.FindGameObjectWithTag("MainCamera");
            if (cameraObj != null)
            {
                mainCamera = cameraObj.GetComponent<Camera>();
            }
        }

        if (mainCamera != null)
        {
            Debug.Log("PlayerAiming found camera");
        }
        else
        {
            Debug.LogWarning("PlayerAiming: No MainCamera found! Make sure your camera is tagged 'MainCamera'");
        }
    }
}