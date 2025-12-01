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
    /// Returns true if player gameplay input (movement, combat, abilities) should be blocked.
    /// This happens when UI menus are open or dialogue is active.
    /// </summary>
    public bool IsPlayerInputBlocked()
    {
        // Check if dialogue is showing
        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueShowing())
        {
            return true;
        }

        // Check if talent tree is open
        TalentTreeUI talentTreeUI = FindObjectOfType<TalentTreeUI>();
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
        {
            return true;
        }

        // Check if inventory is open
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            return true;
        }

        // Add more UI checks here as needed (pause menu, shop, etc.)

        return false; // Input is not blocked
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