using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// UI representation of a single talent node.
/// Spawned dynamically by TalentTreeUI at the node's polar coordinate position.
/// </summary>
public class TalentNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image background;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image border;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("Node Colors")]
    [SerializeField] private Color lockedColor       = new Color(0.15f, 0.15f, 0.15f);
    [SerializeField] private Color aspectLockedColor = new Color(0.25f, 0.15f, 0.30f);
    [SerializeField] private Color availableColor    = new Color(0.2f,  0.8f,  0.2f);
    [SerializeField] private Color learnedColor      = new Color(0.8f,  0.6f,  0.2f);
    [SerializeField] private Color maxedColor        = new Color(1f,    0.85f, 0f);

    [Header("Border Colors by Aspect")]
    [SerializeField] private Color warBorderColor            = new Color(0.9f, 0.2f, 0.2f);
    [SerializeField] private Color sorceryBorderColor        = new Color(0.2f, 0.4f, 0.9f);
    [SerializeField] private Color amplificationBorderColor  = new Color(0.4f, 0.9f, 0.5f);
    [SerializeField] private Color bridgeBorderColor         = new Color(0.9f, 0.9f, 0.2f);

    private TalentNodeData nodeData;
    private TalentTreeUI treeUI;
    private PlayerTalents playerTalents;

    public TalentNodeData NodeData => nodeData;

    public void Initialize(TalentNodeData data, TalentTreeUI tree, PlayerTalents talents)
    {
        nodeData      = data;
        treeUI        = tree;
        playerTalents = talents;

        if (background == null) background = GetComponent<Image>();

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (border != null)
        {
            border.color = data.nodeType == NodeType.Bridge    ? bridgeBorderColor :
                           data.aspect == AspectType.War           ? warBorderColor :
                           data.aspect == AspectType.Sorcery       ? sorceryBorderColor :
                           data.aspect == AspectType.Amplification ? amplificationBorderColor :
                           Color.white;
        }

        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (nodeData == null || playerTalents == null) return;

        int currentRank = playerTalents.GetNodeRank(nodeData);
        bool canLearn   = playerTalents.CanLearnTalent(nodeData);

        if (rankText != null)
        {
            rankText.text = nodeData.maxRank > 1
                ? $"{currentRank}/{nodeData.maxRank}"
                : (currentRank > 0 ? "+" : "");
        }

        if (background != null)
        {
            if (currentRank >= nodeData.maxRank && currentRank > 0)
                background.color = maxedColor;
            else if (currentRank > 0)
                background.color = learnedColor;
            else if (canLearn)
                background.color = availableColor;
            else if (IsAspectLocked())
                background.color = aspectLockedColor;
            else
                background.color = lockedColor;
        }
    }

    bool IsAspectLocked()
    {
        if (playerTalents == null || nodeData == null) return false;
        if (!playerTalents.HasStartingAspect()) return false;
        return nodeData.aspect != playerTalents.GetStartingAspect()
            && nodeData.aspect != AspectType.None
            && !playerTalents.HybridProgressionUnlocked();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (playerTalents == null || nodeData == null) return;

        if (playerTalents.CanLearnTalent(nodeData))
        {
            playerTalents.LearnTalent(nodeData);
            treeUI?.RefreshAllNodes();
            treeUI?.ShowTooltip(nodeData);
        }
        else
        {
            Debug.Log($"Cannot learn {nodeData.nodeName} yet.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData) => treeUI?.ShowTooltip(nodeData);
    public void OnPointerExit(PointerEventData eventData)  => treeUI?.HideTooltip();
}