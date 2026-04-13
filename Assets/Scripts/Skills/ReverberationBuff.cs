using UnityEngine;

/// <summary>
/// Handles the Strength buff granted by Reverberation.
/// Base buff: +20% Strength for 10 seconds.
/// The empowerment talent node increases this based on rank (5/10/15/20%).
/// Re-activating Reverberation while the buff is active refreshes the duration.
/// </summary>
public class ReverberationBuff : MonoBehaviour
{
    [Header("Base Buff Settings")]
    [SerializeField] private float buffDuration = 10f;
    [SerializeField] private float baseStrengthMultiplier = 1.2f; // +20%

    private PlayerStats playerStats;
    private PlayerTalents playerTalents;

    private bool isActive = false;
    private float timeRemaining = 0f;

    // The empowerment node — assign in Inspector once the node is created
    [Header("Empowerment Node (Optional)")]
    [SerializeField] private TalentNodeData empowermentNode;

    void Start()
    {
        playerStats   = GetComponent<PlayerStats>();
        playerTalents = GetComponent<PlayerTalents>();
    }

    void Update()
    {
        if (!isActive) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
            Deactivate();
    }

    // ─── Public API ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Activates or refreshes the buff. Safe to call while already active.
    /// </summary>
    public void Activate()
    {
        timeRemaining = buffDuration;

        if (!isActive)
        {
            isActive = true;
            ApplyBuff();
            Debug.Log($"Reverberation buff activated: +{(GetMultiplier() - 1f) * 100f}% Strength for {buffDuration}s");
        }
        else
        {
            // Refresh — multiplier may have changed if empowerment rank changed
            playerStats?.SetStrengthMultiplier(GetMultiplier());
            Debug.Log($"Reverberation buff refreshed.");
        }
    }

    public bool IsActive() => isActive;
    public float GetTimeRemaining() => timeRemaining;
    public float GetDuration() => buffDuration;

    // ─── Internal ─────────────────────────────────────────────────────────────────

    void ApplyBuff()
    {
        playerStats?.SetStrengthMultiplier(GetMultiplier());
    }

    void Deactivate()
    {
        isActive      = false;
        timeRemaining = 0f;
        playerStats?.SetStrengthMultiplier(1f);
        Debug.Log("Reverberation buff expired.");
    }

    /// <summary>
    /// Returns the current multiplier based on empowerment node rank.
    /// Rank 0 (no empowerment): 1.2 (+20%)
    /// Rank 1: 1.25 (+25%), Rank 2: 1.30, Rank 3: 1.35, Rank 4: 1.40
    /// The empowerment node adds 5% per rank on top of the base 20%.
    /// </summary>
    float GetMultiplier()
    {
        if (empowermentNode == null || playerTalents == null)
            return baseStrengthMultiplier;

        int rank = playerTalents.GetNodeRank(empowermentNode);
        // empowermentNode.effectValue = 5 (5% per rank), so rank 4 = +20% extra
        float extraBonus = (empowermentNode.effectValue * rank) / 100f;
        return baseStrengthMultiplier + extraBonus;
    }
}