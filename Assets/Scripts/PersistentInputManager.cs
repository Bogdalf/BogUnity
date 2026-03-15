using UnityEngine;

/// <summary>
/// Centralized input manager that determines when player input should be blocked.
/// Persists across scenes since it's core to gameplay.
/// All player scripts should check InputManager.Instance.IsPlayerInputBlocked() before processing input.
/// </summary>
public class PersistentInputManager : MonoBehaviour
{
    public static PersistentInputManager Instance { get; private set; }
    

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("InputManager marked as persistent");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Returns true if player gameplay input (movement, gathering, abilities) should be blocked.
    /// This happens when dialogue is active or talent tree is open.
    /// Note: Inventory no longer blocks movement/gathering.
    /// </summary>
    public void SetForcedMovement(bool value)
    {
        forcedMovement = value;
    }

    private bool forcedMovement = false;
    public bool IsForcedMovement()
    {
        return forcedMovement;
    }

    public bool IsPlayerInputBlocked()
    {
        // Add forced movement check
        if (forcedMovement) return true;
        
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueShowing())
            return true;

        TalentTreeUI talentTreeUI = FindFirstObjectByType<TalentTreeUI>();
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
            return true;

        return false;
    }

    /// <summary>
    /// Returns true if combat input (attacks, abilities) should be blocked.
    /// Blocks when dialogue/talents are open, OR when mouse is over inventory UI.
    /// </summary>
    public bool IsCombatInputBlocked()
    {
        // Block combat during dialogue or talent tree
        if (IsPlayerInputBlocked())
        {
            return true;
        }

        // Block combat if mouse is over inventory panel
        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            if (inventoryUI.IsMouseOverInventory())
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true if UI navigation input (closing menus, advancing dialogue) should be allowed.
    /// This is separate from gameplay input.
    /// </summary>
    public bool IsUIInputAllowed()
    {
        // UI input is always allowed (E for dialogue, I for inventory, etc.)
        return true;
    }
}