using UnityEngine;

/// <summary>
/// Manages the player's equipped Rune Book.
/// The Rune Book is a stat stick — no damage ranges or attack speed.
/// It passes flat stat bonuses to PlayerStats, and optionally boosts an Aspect.
/// </summary>
public class PlayerRuneBook : MonoBehaviour
{
    [Header("Equipped Book")]
    [SerializeField] private RuneBookData equippedBook;

    private PlayerStats playerStats;

    // Cached aspect affinity from the current book
    private AspectType currentAspectAffinity = AspectType.None;
    private float currentAspectAffinityBonus = 0f;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        ApplyBookStats();
    }

    // ─── Equip / Unequip ─────────────────────────────────────────────────────────

    public void EquipBook(RuneBookData book)
    {
        equippedBook = book;
        ApplyBookStats();
        Debug.Log(book != null ? "Equipped Rune Book: " + book.itemName : "Rune Book unequipped.");
    }

    public void UnequipBook()
    {
        equippedBook = null;
        ApplyBookStats();
        Debug.Log("Rune Book unequipped.");
    }

    // ─── Internal ─────────────────────────────────────────────────────────────────

    void ApplyBookStats()
    {
        if (playerStats == null) return;

        if (equippedBook != null)
        {
            playerStats.SetRuneBookBonuses(
                equippedBook.bonusStrength,
                equippedBook.bonusIntelligence,
                equippedBook.bonusFocus,
                equippedBook.bonusVitality
            );

            currentAspectAffinity = equippedBook.aspectAffinity;
            currentAspectAffinityBonus = equippedBook.aspectAffinityBonus;
        }
        else
        {
            // No book equipped — clear all bonuses
            playerStats.SetRuneBookBonuses(0f, 0f, 0f, 0f);
            currentAspectAffinity = AspectType.None;
            currentAspectAffinityBonus = 0f;
        }
    }

    // ─── Getters ──────────────────────────────────────────────────────────────────

    public RuneBookData GetEquippedBook() => equippedBook;

    public bool HasBookEquipped() => equippedBook != null;

    /// <summary>
    /// Returns the bonus multiplier this book provides to a given Aspect.
    /// Returns 0 if the book has no affinity or affinity doesn't match.
    /// </summary>
    public float GetAspectAffinityBonus(AspectType aspect)
    {
        if (currentAspectAffinity == AspectType.None) return 0f;
        return currentAspectAffinity == aspect ? currentAspectAffinityBonus : 0f;
    }

    public void RecalculateStats()
    {
        ApplyBookStats();
    }
}