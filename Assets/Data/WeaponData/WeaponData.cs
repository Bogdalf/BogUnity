using UnityEngine;

[CreateAssetMenu(fileName = "NewWeapon", menuName = "Equipment/Weapon")]
public class WeaponData : ItemData  // <-- Changed from ScriptableObject
{
    // REMOVED: public string weaponName; 
    // (now use itemName from ItemData instead)

    [Header("Weapon Info")]
    public WeaponType weaponType;
    public WeaponClass weaponClass;

    [Header("Damage")]
    public float minDamage = 5f;
    public float maxDamage = 10f;

    [Header("Attack Speed")]
    public float attackCooldown = 0.5f;

    [Header("Stat Bonuses")]
    public float bonusStrength = 0f;
    public float bonusVitality = 0f;

    void OnValidate()
    {
        // Auto-set the item type
        itemType = ItemType.Weapon;
        isStackable = false;
        maxStackSize = 1;
    }

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