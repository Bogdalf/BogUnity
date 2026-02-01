using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "Town";

    void Start()
    {
        // Load the first gameplay scene additively
        SceneManager.LoadScene(firstSceneName);
    }
}