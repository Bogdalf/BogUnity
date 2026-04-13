using UnityEngine;

/// <summary>
/// Manages which UI panels can be open simultaneously.
/// Panels register themselves and declare their region.
/// The manager enforces exclusion rules when panels open.
///
/// Regions:
///   TalentTree  — closes everything when opened
///   Right       — Inventory (coexists with everything except TalentTree)
///   Center      — Character Sheet, Spellbook, Town Stash (mutually exclusive)
/// </summary>
public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }

    public enum PanelRegion
    {
        TalentTree,
        Right,
        Center,
    }

    // Currently open panel per region (null = nothing open)
    private IUIPanel talentTreePanel;
    private IUIPanel rightPanel;
    private IUIPanel centerPanel;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// Call this when a panel is about to open.
    /// The manager will close any conflicting panels first.
    /// </summary>
    public void OnPanelOpening(IUIPanel panel, PanelRegion region)
    {
        if (region == PanelRegion.TalentTree)
        {
            // Talent tree closes everything
            talentTreePanel?.Close();
            rightPanel?.Close();
            centerPanel?.Close();

            talentTreePanel = panel;
        }
        else if (region == PanelRegion.Right)
        {
            // Inventory — close talent tree only
            talentTreePanel?.Close();
            talentTreePanel = null;

            rightPanel = panel;
        }
        else if (region == PanelRegion.Center)
        {
            // Center panels close each other and the talent tree
            talentTreePanel?.Close();
            talentTreePanel = null;

            centerPanel?.Close();
            centerPanel = panel;
        }
    }

    /// <summary>
    /// Call this when a panel closes so the manager stops tracking it.
    /// </summary>
    public void OnPanelClosed(IUIPanel panel, PanelRegion region)
    {
        if (region == PanelRegion.TalentTree && talentTreePanel == panel)
            talentTreePanel = null;
        else if (region == PanelRegion.Right && rightPanel == panel)
            rightPanel = null;
        else if (region == PanelRegion.Center && centerPanel == panel)
            centerPanel = null;
    }

    public void CloseAll()
    {
        talentTreePanel?.Close();
        rightPanel?.Close();
        centerPanel?.Close();
        talentTreePanel = null;
        rightPanel      = null;
        centerPanel     = null;
    }
}

/// <summary>
/// Interface for any panel that participates in the panel manager system.
/// </summary>
public interface IUIPanel
{
    void Close();
}