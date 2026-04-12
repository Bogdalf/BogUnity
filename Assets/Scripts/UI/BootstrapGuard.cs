using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Ensures GameBootstrap is loaded additively when entering any gameplay scene directly.
/// Handles both normal game flow (bootstrap already loaded) and 
/// direct scene entry (e.g. hitting Play from the Prison scene in the editor).
/// </summary>
public class BootstrapGuard : MonoBehaviour
{
    [SerializeField] private string bootstrapSceneName = "GameBootstrap";

    void Awake()
    {
        // Check if bootstrap scene is already loaded
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name == bootstrapSceneName)
                return; // Already loaded, nothing to do
        }

        // Not loaded — load it additively so persistent systems exist
        SceneManager.LoadScene(bootstrapSceneName, LoadSceneMode.Additive);
    }
}