using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// The complete talent tree — holds all nodes and global tree settings.
/// One instance of this asset drives both PlayerTalents and TalentTreeUI.
/// Create via: Right Click > Create > Talents > Talent Tree
/// </summary>
[CreateAssetMenu(fileName = "TalentTree", menuName = "Talents/Talent Tree")]
public class TalentTreeData : ScriptableObject
{
    [Header("All Nodes")]
    [Tooltip("Every node in the tree. The UI reads this to build the visual graph.")]
    public List<TalentNodeData> allNodes = new List<TalentNodeData>();

    [Header("Starter Nodes")]
    [Tooltip("The first node the player chooses between when unlocking the talent tree.")]
    public TalentNodeData warStarterNode;
    public TalentNodeData sorceryStarterNode;
    public TalentNodeData amplificationStarterNode;

    [Header("Lock-In Settings")]
    [Tooltip("Points the player must spend in their starting Aspect before hybrid nodes unlock. " +
             "Overridden by the story flag — this is a soft fallback minimum.")]
    public int lockInPointThreshold = 5;

    [Tooltip("GameStateManager flag name that story sets when hybrid progression is allowed.")]
    public string hybridUnlockFlag = "hybrid_unlocked";

    /// <summary>
    /// Returns the starter node for the given Aspect.
    /// </summary>
    public TalentNodeData GetStarterNode(AspectType aspect)
    {
        return aspect switch
        {
            AspectType.War           => warStarterNode,
            AspectType.Sorcery       => sorceryStarterNode,
            AspectType.Amplification => amplificationStarterNode,
            _                        => null
        };
    }

    /// <summary>
    /// Returns all nodes belonging to a specific Aspect (including Bridge nodes that touch it).
    /// </summary>
    public List<TalentNodeData> GetNodesForAspect(AspectType aspect)
    {
        List<TalentNodeData> result = new List<TalentNodeData>();
        foreach (TalentNodeData node in allNodes)
        {
            if (node != null && node.aspect == aspect)
                result.Add(node);
        }
        return result;
    }

    /// <summary>
    /// Returns all Bridge nodes in the tree.
    /// </summary>
    public List<TalentNodeData> GetBridgeNodes()
    {
        List<TalentNodeData> result = new List<TalentNodeData>();
        foreach (TalentNodeData node in allNodes)
        {
            if (node != null && node.nodeType == NodeType.Bridge)
                result.Add(node);
        }
        return result;
    }
}