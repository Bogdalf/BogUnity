using UnityEngine;

/// <summary>
/// Wraps PlayerMelee as an ISkill so the action bar can drive it.
/// This component sits alongside PlayerMelee on the player.
/// On Start it registers itself with ActionBar and locks into LMB.
/// </summary>
public class MeleeSkill : MonoBehaviour, ISkill
{
    [Header("Skill Data")]
    [SerializeField] private SkillData meleeSkillData;

    private PlayerMelee playerMelee;

    public SkillData SkillData => meleeSkillData;

    void Start()
    {
        playerMelee = GetComponent<PlayerMelee>();

        // Lock into LMB automatically — no drag needed
        ActionBar.Instance?.AssignAttackToLMB(meleeSkillData, this);
    }

    public bool CanExecute()
    {
        if (playerMelee == null) return false;
        if (PersistentInputManager.Instance != null && PersistentInputManager.Instance.IsCombatInputBlocked())
            return false;

        return playerMelee.CanAttack();
    }

    public void Execute()
    {
        playerMelee?.TriggerAttack();
    }

    public float GetCooldownPercent()
    {
        return playerMelee != null ? playerMelee.GetCooldownPercent() : 0f;
    }
}