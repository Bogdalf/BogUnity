using System.Collections;
using UnityEngine;

/// <summary>
/// Boss ability that fires meteors at or near the player.
/// Attach to the boss GameObject alongside BossBase/ExampleBossAI.
/// Call StartMeteorAttack() from the boss AI when the attack triggers.
///
/// Each meteor samples the player's position fresh at spawn time,
/// so a moving player gets tracked per meteor rather than all targeting
/// the same initial position.
/// </summary>
public class BossMeteorAttack : MonoBehaviour
{
    public enum MeteorPattern
    {
        Single,        // Each meteor lands directly on the player's current position
        RandomScatter, // Each meteor lands at a random offset from the player
    }

    [Header("Meteor Prefab")]
    [SerializeField] private GameObject meteorPrefab;

    [Header("Pattern")]
    [SerializeField] private MeteorPattern pattern = MeteorPattern.Single;
    [SerializeField] private int   meteorCount  = 1;
    [SerializeField] private float spreadRadius = 2f;
    [SerializeField] private float delayBetween = 0.3f;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 8f;

    private float     lastUseTime = -999f;
    private bool      isActive    = false;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // ─── Public API ───────────────────────────────────────────────────────────────

    public bool CanUse()    => !isActive && Time.time >= lastUseTime + cooldown;
    public bool IsActive()  => isActive;

    public void StartMeteorAttack()
    {
        if (!CanUse()) return;
        StartCoroutine(MeteorAttackRoutine());
    }

    // ─── Routine ──────────────────────────────────────────────────────────────────

    IEnumerator MeteorAttackRoutine()
    {
        isActive    = true;
        lastUseTime = Time.time;

        for (int i = 0; i < meteorCount; i++)
        {
            Vector2 target = GetTargetPosition();
            MeteorProjectile.Spawn(meteorPrefab, target);

            if (delayBetween > 0f && i < meteorCount - 1)
                yield return new WaitForSeconds(delayBetween);
        }

        // Wait for telegraph + fall to complete before marking inactive
        yield return new WaitForSeconds(1f);

        isActive = false;
    }

    // ─── Targeting ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a target position sampled from the player's current position.
    /// Called fresh for each meteor.
    /// </summary>
    Vector2 GetTargetPosition()
    {
        if (player == null) return transform.position;

        Vector2 playerPos = player.position;

        switch (pattern)
        {
            case MeteorPattern.Single:
                return playerPos;

            case MeteorPattern.RandomScatter:
                return playerPos + Random.insideUnitCircle * spreadRadius;

            default:
                return playerPos;
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawWireSphere(player.position, spreadRadius);
    }
}