using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackArc = 90f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("VFX")]
    [SerializeField] private Animator AttackTelegraphs;

    private PlayerStats playerStats;
    private PlayerWarCry playerWarCry;
    private Animator animator;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private Vector3 cachedAttackDirection;
    private bool hitRegistered = false;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerWarCry = GetComponent<PlayerWarCry>();
        animator = GetComponent<Animator>();
    }

    public bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    public void TriggerAttack()
    {
        if (!CanAttack()) return;

        isAttacking = true;
        hitRegistered = false;
        lastAttackTime = Time.time;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cachedAttackDirection = (mousePos - transform.position).normalized;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
            animator.SetFloat("DirectionX", cachedAttackDirection.x);
            animator.SetFloat("DirectionY", cachedAttackDirection.y);
        }

        if (AttackTelegraphs != null)
        {
            AttackTelegraphs.SetTrigger("Slash");
            AttackTelegraphs.SetFloat("DirectionX", cachedAttackDirection.x);
            AttackTelegraphs.SetFloat("DirectionY", cachedAttackDirection.y);
        }
    }

    /// <summary>
    /// Called by Animation Event at the frame where the arc visually lands.
    /// </summary>
    public void OnAttackHit()
    {
        if (hitRegistered) return;
        hitRegistered = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
            float angleToEnemy = Vector3.Angle(cachedAttackDirection, directionToEnemy);

            if (angleToEnemy > attackArc / 2f) continue;

            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null) continue;

            damageable.TakeDamage(CalculateDamage());
        }

        isAttacking = false;
    }

    /// <summary>
    /// Called by Animation Event on the last frame of the attack animation.
    /// Ensures isAttacking resets even if OnAttackHit was never triggered.
    /// </summary>
    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    float CalculateDamage()
    {
        float damage = playerStats != null ? playerStats.GetAttackDamage() : 0f;

        // War Cry damage buff
        if (playerWarCry != null)
            damage *= playerWarCry.GetDamageMultiplier();

        return damage;
    }

    public void SetAttackSpeed(float newCooldown)
    {
        attackCooldown = newCooldown;
    }

    public float GetCooldownPercent()
    {
        if (isAttacking) return 1f;
        float timeSince = Time.time - lastAttackTime;
        if (timeSince >= attackCooldown) return 0f;
        return 1f - (timeSince / attackCooldown);
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}