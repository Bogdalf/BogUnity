using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A single node in the talent tree graph.
/// Position is defined in polar coordinates (angle + radius) so the
/// radial layout can be reconstructed purely from data.
/// Create via: Right Click > Create > Talents > Talent Node
/// </summary>
[CreateAssetMenu(fileName = "NewTalentNode", menuName = "Talents/Talent Node")]
public class TalentNodeData : ScriptableObject
{
    [Header("Identity")]
    public string nodeName = "Unnamed Node";
    [TextArea(2, 4)]
    public string description = "";
    public Sprite icon;
    public AspectType aspect = AspectType.None;
    public NodeType nodeType = NodeType.Minor;

    [Header("Position (Polar Coordinates)")]
    [Tooltip("Angle in degrees. War = 30-150, Amplification = 150-270, Sorcery = 270-390(30)")]
    [Range(0f, 360f)]
    public float angle = 90f;
    [Tooltip("Distance from center. Starter nodes = ~100, outer keystones = ~400+")]
    public float radius = 150f;

    [Header("Effect")]
    public TalentEffectType effectType;
    public float effectValue = 1f;
    public int maxRank = 1;

    [Header("Connections")]
    [Tooltip("All nodes this node is directly connected to. Connections are bidirectional — " +
             "only define them on one side to avoid duplicates.")]
    public List<TalentNodeData> connectedNodes = new List<TalentNodeData>();

    [Header("Requirements")]
    [Tooltip("All of these nodes must have at least 1 rank before this node is reachable.")]
    public List<TalentNodeData> prerequisiteNodes = new List<TalentNodeData>();

    [Tooltip("How many points must be spent in this node's Aspect before it unlocks. " +
             "Set to 0 for starter nodes. Use ~5-10 for early nodes, higher for bridge nodes.")]
    public int aspectPointsRequired = 0;

    [Tooltip("If true, this node can only be reached after the story unlocks hybrid progression.")]
    public bool requiresHybridUnlock = false;
}

public enum NodeType
{
    /// <summary>The first node of an Aspect. Grants the Aspect's signature ability.</summary>
    Starter,
    /// <summary>A major node with a strong, build-defining effect.</summary>
    Keystone,
    /// <summary>A standard talent node.</summary>
    Minor,
    /// <summary>Connects two different Aspects. Requires hybridUnlocked.</summary>
    Bridge,
}