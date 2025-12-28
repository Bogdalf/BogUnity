using UnityEngine;

/// <summary>
/// Centralized input manager that determines when player input should be blocked.
/// All player scripts should check InputManager.Instance.IsPlayerInputBlocked() before processing input.
/// </summary>
public class InputManager : MonoBehaviour
{
    public static InputManager Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
    public bool IsPlayerInputBlocked()
    {
        // Check if dialogue is showing
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueShowing())
        {
            return true;
        }

        // Check if talent tree is open
        TalentTreeUI talentTreeUI = FindFirstObjectByType<TalentTreeUI>();
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
        {
            return true;
        }

        // Inventory no longer blocks all input - only combat clicks
        // Movement and gathering still work with inventory open

        return false; // Input is not blocked
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