using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Equipment/Weapon")]
public class WeaponData : ScriptableObject
{
    [Header("Weapon Info")]
    public string weaponName;
    public WeaponType weaponType;
    public WeaponClass weaponClass;

    [Header("Damage")]
    public float minDamage = 5f;
    public float maxDamage = 10f;

    [Header("Attack Speed")]
    public float attackCooldown = 0.5f; // Time between attacks

    [Header("Stat Bonuses")]
    public float bonusStrength = 0f;
    public float bonusVitality = 0f;

    public float GetRandomDamage()
    {
        return Random.Range(minDamage, maxDamage);
    }
}

public enum WeaponType
{
    TwoHanded,
    OneHanded,
    Shield
}

public enum WeaponClass
{
    Dagger,
    Axe,
    Mace,
    Sword,
    Shield,
    None
}