using UnityEngine;

public class PlayerBuffs : MonoBehaviour
{
    // Axe Frenzy buff
    private int axeFrenzyStacks = 0;
    private float axeFrenzyDuration = 0f;
    private const int MAX_AXE_FRENZY_STACKS = 5;
    private const float AXE_FRENZY_STACK_DURATION = 5f;
    private const float AXE_FRENZY_SPEED_PER_STACK = 5f; // 5% per stack

    private PlayerTalents playerTalents;
    private PlayerEquipment playerEquipment;

    [SerializeField] private TalentData axeFrenzyTalent;
    void Start()
    {
        playerTalents = GetComponent<PlayerTalents>();
        playerEquipment = GetComponent<PlayerEquipment>();
    }

    void Update()
    {
        // Update Axe Frenzy timer
        if (axeFrenzyStacks > 0)
        {
            axeFrenzyDuration -= Time.deltaTime;

            if (axeFrenzyDuration <= 0f)
            {
                // Buff expired
                axeFrenzyStacks = 0;
                axeFrenzyDuration = 0f;
                Debug.Log("Axe Frenzy expired!");

                // Recalculate equipment stats to remove speed bonus
                if (playerEquipment != null)
                {
                    playerEquipment.RecalculateStats();
                }
            }
        }
    }

    // Call this when hitting an enemy with an axe
    public void OnAxeHit()
    {
        // Check if player has the Axe Frenzy talent
        if (!HasAxeFrenzyTalent())
        {
            return;
        }

        // Add a stack (up to max)
        if (axeFrenzyStacks < MAX_AXE_FRENZY_STACKS)
        {
            axeFrenzyStacks++;
            Debug.Log("Axe Frenzy: " + axeFrenzyStacks + " stacks");
        }

        // Refresh duration
        axeFrenzyDuration = AXE_FRENZY_STACK_DURATION;

        // Recalculate equipment stats to apply new speed bonus
        if (playerEquipment != null)
        {
            playerEquipment.RecalculateStats();
        }
    }

    bool HasAxeFrenzyTalent()
    {
        if (playerTalents == null || axeFrenzyTalent == null) return false;

        // Check if player has at least 1 rank in Axe Frenzy
        return playerTalents.GetTalentRank(axeFrenzyTalent) > 0;
    }

    // Get current Axe Frenzy attack speed bonus (as percentage)
    public float GetAxeFrenzySpeedBonus()
    {
        if (axeFrenzyStacks <= 0) return 0f;
        return axeFrenzyStacks * AXE_FRENZY_SPEED_PER_STACK;
    }

    // Getters for UI
    public int GetAxeFrenzyStacks()
    {
        return axeFrenzyStacks;
    }

    public float GetAxeFrenzyDuration()
    {
        return axeFrenzyDuration;
    }

    public bool HasAxeFrenzy()
    {
        return axeFrenzyStacks > 0;
    }
}