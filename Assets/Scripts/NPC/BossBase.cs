using UnityEngine;

/// <summary>
/// Base class for all bosses.
/// Attach to boss GameObjects alongside their specific AI script.
/// KhatunSequence calls TriggerEntrance() when the intro sequence completes.
/// </summary>
public class BossBase : MonoBehaviour
{
    /// <summary>
    /// Called by the scene sequence script when it's time for the boss to appear.
    /// Override this in a derived class or just hook up the UnityEvent below.
    /// </summary>
    public virtual void TriggerEntrance()
    {
        Debug.Log($"{gameObject.name} entrance triggered");
        // Derived boss scripts override this to play entrance animation,
        // start AI, play audio, etc.
    }
}