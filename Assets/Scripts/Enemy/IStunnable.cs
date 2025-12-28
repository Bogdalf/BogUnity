using UnityEngine;

// Interface for entities that can be stunned
public interface IStunnable
{
    void Stun(float duration);
    bool IsStunned();
}