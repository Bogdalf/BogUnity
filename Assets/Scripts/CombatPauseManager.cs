using UnityEngine;

/// <summary>
/// Lightweight static flag that pauses all enemy AI in the scene.
/// No singleton instance needed — enemies just call CombatPauseManager.IsPaused.
///
/// HOW TO USE IN ENEMY AI SCRIPTS:
/// At the top of your Update() or movement/attack logic, add:
///
///     if (CombatPauseManager.IsPaused) return;
///
/// That's all. When the Sun Priestess interjection fires, all enemies freeze instantly.
/// When unpaused, they resume from wherever they were.
///
/// Note: enemies mid-animation will finish their current frame but won't start new
/// actions. For a harder freeze you could also pause the Animator, but for this
/// cinematic moment the soft freeze looks natural.
/// </summary>
public static class CombatPauseManager
{
    public static bool IsPaused { get; private set; } = false;

    /// <summary>
    /// Pause or unpause all enemy AI. Called by SunPriestessInterjection.
    /// </summary>
    public static void SetPaused(bool paused)
    {
        IsPaused = paused;
        Debug.Log($"[CombatPauseManager] IsPaused = {paused}");
    }
}