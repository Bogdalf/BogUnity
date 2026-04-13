using UnityEngine;

[CreateAssetMenu(fileName = "NewRuneBook", menuName = "Equipment/Rune Book")]
public class RuneBookData : ItemData
{
    [Header("Rune Book Info")]
    public string loreDescription;

    [Header("Stat Bonuses")]
    public float bonusStrength = 0f;
    public float bonusIntelligence = 0f;
    public float bonusFocus = 0f;
    public float bonusVitality = 0f;

    [Header("Aspect Affinity (Optional)")]
    [Tooltip("Boosts a specific Aspect's power scaling. Leave at None for a neutral book.")]
    public AspectType aspectAffinity = AspectType.None;
    public float aspectAffinityBonus = 0f;

    void OnValidate()
    {
        itemType = ItemType.RuneBook;
        isStackable = false;
        maxStackSize = 1;
    }
}