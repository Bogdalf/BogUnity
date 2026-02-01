using UnityEngine;

/// <summary>
/// Marks the UI Canvas to persist across scene loads.
/// This canvas should contain all persistent UI elements like:
/// - Inventory
/// - Equipment
/// - Talent Tree
/// - Dialogue
/// - Health/Mana bars
/// </summary>
public class PersistentUICanvas : MonoBehaviour
{
    public static PersistentUICanvas Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject talentTreePanel;
    [SerializeField] private GameObject dialoguePanel;

    void Awake()
    {
        // Singleton pattern - only one UI canvas can exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("UI Canvas marked as persistent");
        }
        else
        {
            // Destroy duplicate UI canvases
            Debug.LogWarning("Duplicate UI canvas detected - destroying");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Call this to close all UI panels (useful when transitioning scenes)
    /// </summary>
    public void CloseAllPanels()
    {
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (talentTreePanel != null)
            talentTreePanel.SetActive(false);

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Debug.Log("All UI panels closed");
    }

    /// <summary>
    /// Check if any blocking UI is open (for input management)
    /// </summary>
    public bool IsAnyBlockingUIOpen()
    {
        bool inventoryOpen = inventoryPanel != null && inventoryPanel.activeSelf;
        bool talentOpen = talentTreePanel != null && talentTreePanel.activeSelf;
        bool dialogueOpen = dialoguePanel != null && dialoguePanel.activeSelf;

        return inventoryOpen || talentOpen || dialogueOpen;
    }
}