using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Handles scene transitions and ensures player spawns at the correct location.
/// Place this on trigger zones, buttons, or call it from other scripts.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private Vector3 spawnPosition = Vector3.zero;
    [SerializeField] private bool useSpawnPoint = true; // If true, scene must have a PlayerSpawnPoint

    [Header("Trigger Settings (Optional)")]
    [SerializeField] private bool triggerOnEnter = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool showPrompt = true;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && !triggerOnEnter)
        {
            if (Input.GetKeyDown(interactKey))
            {
                LoadScene();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;

            if (triggerOnEnter)
            {
                LoadScene();
            }
            else if (showPrompt)
            {
                Debug.Log($"Press {interactKey} to travel to {targetSceneName}");
                // TODO: Show UI prompt
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            // TODO: Hide UI prompt
        }
    }

    /// <summary>
    /// Load the target scene. Can be called from buttons or other scripts.
    /// </summary>
    public void LoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("SceneTransition: No target scene name set!");
            return;
        }

        // Close any open UI panels before transitioning
        if (PersistentUICanvas.Instance != null)
        {
            PersistentUICanvas.Instance.CloseAllPanels();
        }

        // If not using spawn point, manually set position after load
        if (!useSpawnPoint)
        {
            StartCoroutine(LoadSceneAndSetPosition());
        }
        else
        {
            // Just load the scene - PlayerSpawnPoint in the scene will handle positioning
            SceneManager.LoadScene(targetSceneName);
        }
    }

    IEnumerator LoadSceneAndSetPosition()
    {
        // Load the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);

        // Wait for scene to finish loading
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Set player position
        if (PersistentPlayer.Instance != null)
        {
            PersistentPlayer.Instance.SetPosition(spawnPosition);
        }
    }

    /// <summary>
    /// Static helper method to load a scene from anywhere
    /// </summary>
    public static void LoadSceneStatic(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Draw gizmo to show where this transition leads
    void OnDrawGizmos()
    {
        if (GetComponent<Collider2D>() != null)
        {
            Gizmos.color = new Color(0f, 1f, 1f, 0.3f); // Cyan
            Gizmos.DrawCube(transform.position, transform.localScale);
        }
    }
}