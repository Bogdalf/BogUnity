using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Centralized input manager. Single source of truth for all input blocking.
/// Lives in GameBootstrap and persists across scenes.
/// </summary>
public class PersistentInputManager : MonoBehaviour
{
    public static PersistentInputManager Instance { get; private set; }

    private bool forcedMovement  = false;
    private bool sequenceBlocked = false;

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
    public void SetSequenceBlocked(bool value) => sequenceBlocked = value;

    // Kept for backwards compatibility — no longer affect blocking logic
    public void SetSpellbookOpen(bool value)  { }
    public void SetStashOpen(bool value)      { }
    public void SetStatSheetOpen(bool value)  { }

    // ─── Queries ──────────────────────────────────────────────────────────────────

    public bool IsForcedMovement()  => forcedMovement;
    public bool IsSequenceBlocked() => sequenceBlocked;

    /// <summary>
    /// True when all gameplay input should be blocked.
    /// Currently only dialogue, cutscenes, and the talent tree (navigated by click).
    /// UI panels like inventory, stash, and stat sheet no longer block movement.
    /// </summary>
    public bool IsPlayerInputBlocked()
    {
        if (forcedMovement)  return true;
        if (sequenceBlocked) return true;

        if (DialogueUI.Instance != null && DialogueUI.Instance.IsDialogueShowing())
            return true;

        TalentTreeUI talentTreeUI = FindFirstObjectByType<TalentTreeUI>();
        if (talentTreeUI != null && talentTreeUI.IsTalentTreeOpen())
            return true;

        return false;
    }

    /// <summary>
    /// True when combat input (LMB attack) should be blocked.
    /// Blocks when mouse is physically over any UI element — inventory, stash,
    /// spellbook, stat sheet, etc. Movement is NOT affected by this check.
    /// </summary>
    public bool IsCombatInputBlocked()
    {
        if (IsPlayerInputBlocked()) return true;

        // Block LMB only when the cursor is over any UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;

        return false;
    }

    public bool IsUIInputAllowed() => true;
}