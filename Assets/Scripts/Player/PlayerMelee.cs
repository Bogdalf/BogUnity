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
    private PlayerEquipment playerEquipment;
    private PlayerBuffs playerBuffs;
    private PlayerWarCry playerWarCry;
    private Animator animator;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private Vector3 cachedAttackDirection;
    private bool hitRegistered = false;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerEquipment = GetComponent<PlayerEquipment>();
        playerBuffs = GetComponent<PlayerBuffs>();
        playerWarCry = GetComponent<PlayerWarCry>();
        animator = GetComponent<Animator>();
    }

    // Update no longer handles input — ActionBar drives this via MeleeSkill
    // CanAttack and TriggerAttack are public for MeleeSkill to call

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
            animator.SetTrigger("Attack");
        if (AttackTelegraphs != null)
            AttackTelegraphs.SetTrigger("Slash");
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

            if (playerEquipment != null && playerEquipment.IsDualWielding())
            {
                float baseAttack = playerStats != null ? playerStats.GetAttackDamage() : 0f;
                float mainHandDamage = CalculateDamageWithMastery(
                    baseAttack,
                    playerEquipment.GetMainHandDamage(),
                    playerEquipment.GetMainHandWeaponClass()
                );
                damageable.TakeDamage(mainHandDamage);
                StartCoroutine(DualWieldSecondHit(damageable, baseAttack));
            }
            else
            {
                float baseAttack = playerStats != null ? playerStats.GetAttackDamage() : 0f;
                float weaponDamage = playerEquipment != null ? playerEquipment.GetWeaponDamage() : 0f;
                WeaponClass weaponClass = playerEquipment != null
                    ? playerEquipment.GetMainHandWeaponClass()
                    : WeaponClass.None;

                float totalDamage = CalculateDamageWithMastery(baseAttack, weaponDamage, weaponClass);
                damageable.TakeDamage(totalDamage);
            }

            if (playerEquipment != null && playerBuffs != null)
            {
                if (playerEquipment.GetMainHandWeaponClass() == WeaponClass.Axe)
                    playerBuffs.OnAxeHit();

                if (playerEquipment.IsDualWielding() && playerEquipment.GetOffHandWeaponClass() == WeaponClass.Axe)
                    playerBuffs.OnAxeHit();
            }
        }

        isAttacking = false;
    }

    System.Collections.IEnumerator DualWieldSecondHit(IDamageable damageable, float baseAttack)
    {
        yield return new WaitForSeconds(0.1f);

        if (damageable != null && playerEquipment != null)
        {
            float offHandDamage = CalculateDamageWithMastery(
                baseAttack,
                playerEquipment.GetOffHandDamage(),
                playerEquipment.GetOffHandWeaponClass()
            );
            damageable.TakeDamage(offHandDamage);
        }
    }

    float CalculateDamageWithMastery(float baseAttack, float weaponDamage, WeaponClass weaponClass)
    {
        float totalDamage = baseAttack + weaponDamage;

        PlayerTalents talents = GetComponent<PlayerTalents>();
        if (talents != null && weaponClass != WeaponClass.None)
        {
            float masteryBonus = talents.GetWeaponMasteryDamageBonus(weaponClass);
            if (masteryBonus > 0)
                totalDamage += totalDamage * (masteryBonus / 100f);
        }

        if (playerWarCry != null)
        {
            float warCryMultiplier = playerWarCry.GetDamageMultiplier();
            if (warCryMultiplier > 1f)
                totalDamage *= warCryMultiplier;
        }

        return totalDamage;
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