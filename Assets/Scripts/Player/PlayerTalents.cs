using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all talent state for the player.
/// Reads from TalentTreeData for graph structure and rules.
/// </summary>
public class PlayerTalents : MonoBehaviour
{
    [Header("Talent Tree")]
    [SerializeField] private TalentTreeData talentTree;

    [Header("Talent Points")]
    [SerializeField] private int availableTalentPoints = 0;

    // Learned nodes and their current rank
    private Dictionary<TalentNodeData, int> learnedNodes = new Dictionary<TalentNodeData, int>();

    // Points spent per Aspect (tracked separately for lock-in logic)
    private Dictionary<AspectType, int> aspectPointsSpent = new Dictionary<AspectType, int>
    {
        { AspectType.War,           0 },
        { AspectType.Sorcery,       0 },
        { AspectType.Amplification, 0 },
    };

    // The Aspect the player started with (set on first node learned)
    private AspectType startingAspect = AspectType.None;

    private PlayerStats playerStats;
    private PlayerDash playerDash;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerDash  = GetComponent<PlayerDash>();
    }

    // ─── Unlock Tree ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Called by story/game event when the player first unlocks the talent tree.
    /// Grants the first point so the player can immediately choose a starter node.
    /// </summary>
    public void UnlockTalentTree()
    {
        availableTalentPoints = Mathf.Max(availableTalentPoints, 1);
        Debug.Log("Talent tree unlocked!");
    }

    /// <summary>
    /// Grant additional talent points (on level up, story reward, etc.)
    /// </summary>
    public void GrantTalentPoints(int amount)
    {
        availableTalentPoints += amount;
        Debug.Log($"Granted {amount} talent points. Total available: {availableTalentPoints}");
    }

    // ─── Can Learn ────────────────────────────────────────────────────────────────

    public bool CanLearnTalent(TalentNodeData node)
    {
        if (node == null)                            return false;
        if (availableTalentPoints <= 0)              return false;
        if (GetNodeRank(node) >= node.maxRank)       return false;

        // Starter nodes have their own rules
        if (node.nodeType == NodeType.Starter)
            return CanLearnStarterNode(node);

        // All non-starter nodes need at least one prerequisite learned
        if (!HasAnyPrerequisite(node)) return false;

        // Check aspect points requirement
        if (node.aspectPointsRequired > 0)
        {
            int spentInAspect = GetAspectPointsSpent(node.aspect);
            if (spentInAspect < node.aspectPointsRequired) return false;
        }

        // Bridge nodes and nodes outside starting aspect require hybrid unlock
        if (node.requiresHybridUnlock || IsOutsideStartingAspect(node))
        {
            if (!IsHybridUnlocked()) return false;
        }

        // Lock-in: if hybrid isn't unlocked yet, can't spend outside starting aspect
        if (!IsHybridUnlocked() && startingAspect != AspectType.None)
        {
            if (node.aspect != startingAspect && node.nodeType != NodeType.Bridge)
                return false;
        }

        return true;
    }

    bool CanLearnStarterNode(TalentNodeData node)
    {
        // No aspect chosen yet — all three starters are available (player picks one)
        if (startingAspect == AspectType.None) return true;

        // Aspect already chosen — only the matching starter matters
        return node.aspect == startingAspect && GetNodeRank(node) < node.maxRank;
    }

    bool HasAnyPrerequisite(TalentNodeData node)
    {
        if (node.prerequisiteNodes == null || node.prerequisiteNodes.Count == 0)
            return false;

        foreach (TalentNodeData prereq in node.prerequisiteNodes)
        {
            if (prereq != null && GetNodeRank(prereq) > 0)
                return true;
        }
        return false;
    }

    bool IsOutsideStartingAspect(TalentNodeData node)
    {
        if (startingAspect == AspectType.None) return false;
        return node.aspect != startingAspect && node.aspect != AspectType.None;
    }

    bool IsHybridUnlocked()
    {
        if (talentTree == null) return false;

        // Story flag takes priority
        if (GameStateManager.Instance != null &&
            GameStateManager.Instance.GetQuestFlag(talentTree.hybridUnlockFlag))
            return true;

        // Fallback: enough points spent in starting aspect
        if (startingAspect != AspectType.None)
            return GetAspectPointsSpent(startingAspect) >= talentTree.lockInPointThreshold;

        return false;
    }

    // ─── Learn ────────────────────────────────────────────────────────────────────

    public void LearnTalent(TalentNodeData node)
    {
        if (!CanLearnTalent(node)) return;

        // Lock in starting aspect on first starter node
        if (startingAspect == AspectType.None && node.nodeType == NodeType.Starter)
            startingAspect = node.aspect;

        if (learnedNodes.ContainsKey(node))
            learnedNodes[node]++;
        else
            learnedNodes[node] = 1;

        availableTalentPoints--;

        // Track aspect points spent
        if (node.aspect != AspectType.None && aspectPointsSpent.ContainsKey(node.aspect))
            aspectPointsSpent[node.aspect]++;

        ApplyNodeEffect(node);

        Debug.Log($"Learned: {node.nodeName} (Rank {learnedNodes[node]}) — {node.aspect}");
    }

    void ApplyNodeEffect(TalentNodeData node)
    {
        switch (node.effectType)
        {
            case TalentEffectType.IncreaseStrength:
                playerStats?.AddStrength(node.effectValue);
                break;
            case TalentEffectType.IncreaseIntelligence:
                playerStats?.AddIntelligence(node.effectValue);
                break;
            case TalentEffectType.IncreaseFocus:
                playerStats?.AddFocus(node.effectValue);
                break;
            case TalentEffectType.IncreaseVitality:
                playerStats?.AddVitality(node.effectValue);
                break;

            // Aspect damage bonuses are read dynamically at ability fire time
            case TalentEffectType.WarDamageBonus:
            case TalentEffectType.SorceryDamageBonus:
            case TalentEffectType.AmplificationBonus:
                break;

            // Dash reads these via GetDashDistanceBonus / GetDashCooldownReduction
            case TalentEffectType.IncreaseDashDistance:
            case TalentEffectType.DecreaseDashCooldown:
                break;

            default:
                Debug.Log($"Talent effect not yet implemented: {node.effectType}");
                break;
        }
    }

    // ─── Queries ──────────────────────────────────────────────────────────────────

    public int GetNodeRank(TalentNodeData node)
    {
        if (node == null) return 0;
        return learnedNodes.ContainsKey(node) ? learnedNodes[node] : 0;
    }

    public int GetAvailableTalentPoints()        => availableTalentPoints;
    public AspectType GetStartingAspect()        => startingAspect;
    public bool HasStartingAspect()              => startingAspect != AspectType.None;
    public bool HybridProgressionUnlocked()      => IsHybridUnlocked();

    public int GetAspectPointsSpent(AspectType aspect)
    {
        return aspectPointsSpent.ContainsKey(aspect) ? aspectPointsSpent[aspect] : 0;
    }

    /// <summary>
    /// Total % damage bonus from talents for a given Aspect.
    /// Called by Aspect ability scripts at damage calculation time.
    /// </summary>
    public float GetAspectDamageBonus(AspectType aspect)
    {
        TalentEffectType targetEffect = aspect switch
        {
            AspectType.War           => TalentEffectType.WarDamageBonus,
            AspectType.Sorcery       => TalentEffectType.SorceryDamageBonus,
            AspectType.Amplification => TalentEffectType.AmplificationBonus,
            _                        => (TalentEffectType)(-1)
        };

        float total = 0f;
        foreach (var kvp in learnedNodes)
            if (kvp.Key.effectType == targetEffect)
                total += kvp.Key.effectValue * kvp.Value;
        return total;
    }

    public float GetDashDistanceBonus()
    {
        float total = 0f;
        foreach (var kvp in learnedNodes)
            if (kvp.Key.effectType == TalentEffectType.IncreaseDashDistance)
                total += kvp.Key.effectValue * kvp.Value;
        return total;
    }

    public float GetDashCooldownReduction()
    {
        float total = 0f;
        foreach (var kvp in learnedNodes)
            if (kvp.Key.effectType == TalentEffectType.DecreaseDashCooldown)
                total += kvp.Key.effectValue * kvp.Value;
        return total;
    }

    public TalentTreeData GetTalentTree() => talentTree;
}