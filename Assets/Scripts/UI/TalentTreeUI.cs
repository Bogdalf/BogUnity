using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Builds and manages the radial talent tree UI.
/// Reads TalentTreeData to spawn TalentNodeUI buttons at polar coordinate positions
/// and draws lines between connected nodes.
///
/// Layout:
///   War           = 30° – 150°  (top)
///   Amplification = 150° – 270° (bottom-left)
///   Sorcery       = 270° – 30°  (bottom-right)
///
/// Setup requirements:
///   - nodeContainer: RectTransform centered in the panel, anchored at center.
///     Nodes spawn relative to its origin (the tree center).
///   - lineContainer: sibling of nodeContainer, placed behind it in hierarchy.
///   - talentNodePrefab: small button prefab with TalentNodeUI component.
///   - linePrefab: a GameObject with an Image component (plain white 1x1 sprite).
/// </summary>
public class TalentTreeUI : MonoBehaviour, IUIPanel
{
    [Header("References")]
    [SerializeField] private PlayerTalents playerTalents;
    [SerializeField] private GameObject talentTreePanel;
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private TalentTreePanner panner;

    [Header("Prefabs")]
    [SerializeField] private GameObject talentNodePrefab;
    [SerializeField] private GameObject linePrefab;

    [Header("Lines")]
    [SerializeField] private Color lineColorDefault = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    [SerializeField] private Color lineColorLearned = new Color(0.85f, 0.75f, 0.3f, 1f);
    [SerializeField] private float lineThickness = 3f;

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipNameText;
    [SerializeField] private TextMeshProUGUI tooltipDescText;
    [SerializeField] private TextMeshProUGUI tooltipRankText;
    [SerializeField] private TextMeshProUGUI tooltipEffectText;
    [SerializeField] private TextMeshProUGUI tooltipRequirementText;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI talentPointsText;
    [SerializeField] private TextMeshProUGUI startingAspectText;

    [Header("Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.N;
    [SerializeField] private float nodeSize = 60f;

    private bool isTalentTreeOpen = false;
    private bool treeBuilt = false;

    private Dictionary<TalentNodeData, TalentNodeUI> spawnedNodes
        = new Dictionary<TalentNodeData, TalentNodeUI>();
    private Dictionary<string, Image> spawnedLines
        = new Dictionary<string, Image>();

    public bool IsTalentTreeOpen() => isTalentTreeOpen;

    // ─── Lifecycle ────────────────────────────────────────────────────────────────

    void Start()
    {
        FindPlayerReferences();

        if (tooltipPanel != null)   tooltipPanel.SetActive(false);
        if (talentTreePanel != null) talentTreePanel.SetActive(false);
    }

    void Update()
    {
        if (playerTalents == null) FindPlayerReferences();

        if (Input.GetKeyDown(toggleKey))
            ToggleTalentTree();

        // Keep tooltip following mouse while visible
        if (tooltipPanel != null && tooltipPanel.activeSelf)
            RepositionTooltip();
    }

    void RepositionTooltip()
    {
        if (!(tooltipPanel.transform is RectTransform tooltipRT)) return;

        Canvas rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos
        );

        tooltipRT.anchoredPosition = localPos + new Vector2(-48f, 38f);
    }

    void FindPlayerReferences()
    {
        if (playerTalents != null) return;

        GameObject player = PersistentPlayer.Instance != null
            ? PersistentPlayer.Instance.gameObject
            : GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            playerTalents = player.GetComponent<PlayerTalents>();

        if (playerTalents == null)
            Debug.LogWarning("TalentTreeUI: Could not find PlayerTalents!");
    }

    // ─── Toggle ───────────────────────────────────────────────────────────────────

    void ToggleTalentTree()
    {
        if (isTalentTreeOpen)
            Close();
        else
            Open();
    }

    void Open()
    {
        isTalentTreeOpen = true;

        UIPanelManager.Instance?.OnPanelOpening(this, UIPanelManager.PanelRegion.TalentTree);

        if (talentTreePanel != null)
            talentTreePanel.SetActive(true);

        // Reset to centered 1x view every time the tree opens
        panner?.ResetView();

        BuildTree();
        RefreshAllNodes();
    }

    public void Close()
    {
        isTalentTreeOpen = false;

        if (talentTreePanel != null)
            talentTreePanel.SetActive(false);

        HideTooltip();

        UIPanelManager.Instance?.OnPanelClosed(this, UIPanelManager.PanelRegion.TalentTree);
    }

    // ─── Build ────────────────────────────────────────────────────────────────────

    void BuildTree()
    {
        if (treeBuilt) return;
        if (playerTalents == null) return;

        TalentTreeData tree = playerTalents.GetTalentTree();
        if (tree == null)
        {
            Debug.LogWarning("TalentTreeUI: No TalentTreeData assigned to PlayerTalents!");
            return;
        }

        // Spawn all node buttons
        foreach (TalentNodeData node in tree.allNodes)
        {
            if (node != null)
                SpawnNode(node);
        }

        // Draw connections (after all nodes exist so positions are available)
        foreach (TalentNodeData node in tree.allNodes)
        {
            if (node == null) continue;
            foreach (TalentNodeData connected in node.connectedNodes)
            {
                if (connected != null)
                    DrawConnection(node, connected);
            }
        }

        treeBuilt = true;
    }

    void SpawnNode(TalentNodeData node)
    {
        if (talentNodePrefab == null || nodeContainer == null) return;

        GameObject obj = Instantiate(talentNodePrefab, nodeContainer);
        obj.name = node.nodeName;

        RectTransform rt = obj.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta        = new Vector2(nodeSize, nodeSize);
            rt.anchoredPosition = PolarToCanvas(node.angle, node.radius);
        }

        TalentNodeUI nodeUI = obj.GetComponent<TalentNodeUI>();
        if (nodeUI != null)
        {
            nodeUI.Initialize(node, this, playerTalents);
            spawnedNodes[node] = nodeUI;
        }
    }

    void DrawConnection(TalentNodeData from, TalentNodeData to)
    {
        string key = GetConnectionKey(from, to);
        if (spawnedLines.ContainsKey(key)) return;
        if (linePrefab == null || lineContainer == null) return;

        Vector2 posA = PolarToCanvas(from.angle, from.radius);
        Vector2 posB = PolarToCanvas(to.angle,   to.radius);

        GameObject lineObj   = Instantiate(linePrefab, lineContainer);
        Image      lineImage = lineObj.GetComponent<Image>();
        if (lineImage == null) return;

        RectTransform rt  = lineObj.GetComponent<RectTransform>();
        Vector2 direction = posB - posA;
        float length      = direction.magnitude;

        rt.sizeDelta        = new Vector2(length, lineThickness);
        rt.anchoredPosition = posA + direction * 0.5f;
        rt.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
        rt.pivot            = new Vector2(0.5f, 0.5f);

        lineImage.color   = lineColorDefault;
        lineImage.raycastTarget = false;

        spawnedLines[key] = lineImage;
    }

    // ─── Refresh ──────────────────────────────────────────────────────────────────

    public void RefreshAllNodes()
    {
        foreach (var kvp in spawnedNodes)
            kvp.Value?.UpdateDisplay();

        RefreshLines();
        UpdateHeader();
    }

    void RefreshLines()
    {
        if (playerTalents == null) return;

        TalentTreeData tree = playerTalents.GetTalentTree();
        if (tree == null) return;

        foreach (TalentNodeData node in tree.allNodes)
        {
            if (node == null) continue;
            foreach (TalentNodeData connected in node.connectedNodes)
            {
                if (connected == null) continue;
                string key = GetConnectionKey(node, connected);
                if (!spawnedLines.ContainsKey(key)) continue;

                bool bothLearned = playerTalents.GetNodeRank(node)      > 0
                                && playerTalents.GetNodeRank(connected) > 0;

                spawnedLines[key].color = bothLearned ? lineColorLearned : lineColorDefault;
            }
        }
    }

    void UpdateHeader()
    {
        if (talentPointsText != null && playerTalents != null)
            talentPointsText.text = "Points: " + playerTalents.GetAvailableTalentPoints();

        if (startingAspectText != null && playerTalents != null)
        {
            AspectType aspect = playerTalents.GetStartingAspect();
            startingAspectText.text = aspect == AspectType.None
                ? "Choose your Aspect"
                : aspect.ToString();
        }
    }

    // ─── Tooltip ──────────────────────────────────────────────────────────────────

    public void ShowTooltip(TalentNodeData node)
    {
        if (tooltipPanel == null || node == null) return;

        tooltipPanel.SetActive(true);

        if (tooltipNameText != null)   tooltipNameText.text   = node.nodeName;
        if (tooltipDescText != null)   tooltipDescText.text   = node.description;

        if (tooltipRankText != null && playerTalents != null)
            tooltipRankText.text = $"Rank: {playerTalents.GetNodeRank(node)} / {node.maxRank}";

        if (tooltipEffectText != null)
            tooltipEffectText.text = GetEffectDescription(node);

        if (tooltipRequirementText != null)
            tooltipRequirementText.text = GetRequirementDescription(node);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────────

    /// <summary>
    /// Converts polar coordinates to canvas anchored position.
    /// 90° = top of screen (War region center), consistent with Unity's coordinate system.
    /// </summary>
    Vector2 PolarToCanvas(float angleDegrees, float radius)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad) * radius, Mathf.Sin(rad) * radius);
    }

    string GetConnectionKey(TalentNodeData a, TalentNodeData b)
    {
        return string.Compare(a.name, b.name, System.StringComparison.Ordinal) < 0
            ? a.name + "|" + b.name
            : b.name + "|" + a.name;
    }

    string GetEffectDescription(TalentNodeData node)
    {
        int   rank  = playerTalents != null ? playerTalents.GetNodeRank(node) : 0;
        float total = node.effectValue * Mathf.Max(1, rank);

        return node.effectType switch
        {
            TalentEffectType.IncreaseStrength          => $"+{total} Strength",
            TalentEffectType.IncreaseIntelligence      => $"+{total} Intelligence",
            TalentEffectType.IncreaseFocus             => $"+{total} Focus",
            TalentEffectType.IncreaseVitality          => $"+{total} Vitality",
            TalentEffectType.WarDamageBonus            => $"+{total}% War Ability Damage",
            TalentEffectType.SorceryDamageBonus        => $"+{total}% Sorcery Ability Damage",
            TalentEffectType.AmplificationBonus        => $"+{total}% Amplification Potency",
            TalentEffectType.IncreaseDashDistance      => $"+{total}% Dash Distance",
            TalentEffectType.DecreaseDashCooldown      => $"-{total}% Dash Cooldown",
            TalentEffectType.IncreaseMeleeRange        => $"+{total}% Melee Range",
            TalentEffectType.IncreaseMeleeArc          => $"+{total}% Melee Arc",
            TalentEffectType.ReverberationRadiusBonus  => $"+{total} Reverberation Radius",
            TalentEffectType.ReverberationEmpowerment  => $"+{node.effectValue * rank}% Reverberation Strength Buff",
            TalentEffectType.ReverberationEchoPulse    => (null),
            _ => node.effectType.ToString()
        };
    }

    string GetRequirementDescription(TalentNodeData node)
    {
        if (playerTalents == null) return "";
        if (node.nodeType == NodeType.Starter) return "Choose this as your starting Aspect";

        var lines = new List<string>();

        if (node.aspectPointsRequired > 0)
        {
            int spent = playerTalents.GetAspectPointsSpent(node.aspect);
            if (spent < node.aspectPointsRequired)
                lines.Add($"Requires {node.aspectPointsRequired} {node.aspect} points ({spent}/{node.aspectPointsRequired})");
        }

        if (node.requiresHybridUnlock && !playerTalents.HybridProgressionUnlocked())
            lines.Add("Requires: Hybrid progression unlocked");

        if (node.prerequisiteNodes != null)
            foreach (TalentNodeData prereq in node.prerequisiteNodes)
                if (prereq != null && playerTalents.GetNodeRank(prereq) == 0)
                    lines.Add($"Requires: {prereq.nodeName}");

        return lines.Count > 0
            ? string.Join("\n", lines)
            : (playerTalents.CanLearnTalent(node) ? "Ready to learn" : "");
    }
}