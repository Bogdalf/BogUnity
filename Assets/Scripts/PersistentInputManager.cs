using UnityEngine;

/// <summary>
/// Centralized input manager. Single source of truth for all input blocking.
/// Lives in GameBootstrap and persists across scenes.
/// </summary>
public class PersistentInputManager : MonoBehaviour
{
    public static PersistentInputManager Instance { get; private set; }

    private bool forcedMovement = false;
    private bool spellbookOpen = false;
    private bool sequenceBlocked = false;
    private bool stashOpen = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ─── Setters ──────────────────────────────────────────────────────────────────

    public void SetForcedMovement(bool value)  => forcedMovement  = value;
    public void SetSpellbookOpen(bool value)   => spellbookOpen   = value;
    public void SetSequenceBlocked(bool value) => sequenceBlocked = value;
    public void SetStashOpen(bool value)       => stashOpen       = value;

    // ─── Queries ──────────────────────────────────────────────────────────────────

    public bool IsForcedMovement()   => forcedMovement;
    public bool IsSequenceBlocked()  => sequenceBlocked;

    /// <summary>
    /// True when all gameplay input should be blocked.
    /// </summary>
    public bool IsPlayerInputBlocked()
    {
        if (forcedMovement)   return true;
        if (spellbookOpen)    return true;
        if (sequenceBlocked)  return true;
        if (stashOpen)        return true;

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueShowing())
            return true;

        TalentTreeUI talentTreeUI = FindFirstObjectByType<TalentTreeUI>();
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
            return true;

        return false;
    }

    /// <summary>
    /// True when combat input should be blocked.
    /// Superset of IsPlayerInputBlocked.
    /// </summary>
    public bool IsCombatInputBlocked()
    {
        if (IsPlayerInputBlocked()) return true;

        InventoryUI inventoryUI = FindFirstObjectByType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen() && inventoryUI.IsMouseOverInventory())
            return true;

        return false;
    }

    public bool IsUIInputAllowed() => true;
}