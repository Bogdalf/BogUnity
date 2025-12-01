using UnityEngine;

// Interface that all damageable entities (enemies, bosses, destructibles) can implement
public interface IDamageable
{
    void TakeDamage(float damage);
}