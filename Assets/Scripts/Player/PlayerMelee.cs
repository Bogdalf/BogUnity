using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackArc = 90f; // Degrees of arc
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("Visual")]
    [SerializeField] private GameObject slashVisualObject;
    [SerializeField] private float slashDuration = 0.2f;

    private TalentTreeUI talentTreeUI;
    private InventoryUI inventoryUI;

    private PlayerStats playerStats;
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private PlayerEquipment playerEquipment;
    private PlayerBuffs playerBuffs;
    private PlayerWarCry playerWarCry;

    void Start()
    {
        playerStats = GetComponent<PlayerStats>();
        playerEquipment = GetComponent<PlayerEquipment>();
        playerBuffs = GetComponent<PlayerBuffs>();
        playerWarCry = GetComponent<PlayerWarCry>();
    }

    void Update()
    {
        // Check centralized input manager FIRST before any input processing
        if (InputManager.Instance != null && InputManager.Instance.IsCombatInputBlocked())
        {
            return;
        }

        // Now safe to process combat input
        if (Input.GetMouseButton(0) && CanAttack())
        {
            Attack();
        }
    }

    public void SetAttackSpeed(float newCooldown)
    {
        attackCooldown = newCooldown;
        Debug.Log("Attack speed set to: " + attackCooldown);
    }

    bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Get direction player is facing (toward mouse)
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = (mousePos - transform.position).normalized;

        // Find all enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);

        Debug.Log("Found " + hits.Length + " colliders in range");

        foreach (Collider2D hit in hits)
        {
            Debug.Log("Hit object: " + hit.gameObject.name + " with tag: " + hit.tag);

            if (hit.CompareTag("Enemy"))
            {
                // Check if enemy is within attack arc
                Vector3 directionToEnemy = (hit.transform.position - transform.position).normalized;
                float angleToEnemy = Vector3.Angle(attackDirection, directionToEnemy);

                Debug.Log("Enemy angle: " + angleToEnemy + " degrees (arc is " + (attackArc / 2f) + ")");

                if (angleToEnemy <= attackArc / 2f)
                {
                    Debug.Log("ENEMY IN ARC - DEALING DAMAGE!");

                    // Use IDamageable interface instead of specific enemy type
                    IDamageable damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        // Check if dual wielding
                        if (playerEquipment != null && playerEquipment.IsDualWielding())
                        {
                            // Dual wield: Two separate damage instances
                            float baseAttack = playerStats != null ? playerStats.GetAttackDamage() : 0f;

                            // First hit - main hand (with mastery bonus)
                            float mainHandDamage = CalculateDamageWithMastery(baseAttack, playerEquipment.GetMainHandDamage(), playerEquipment.GetMainHandWeaponClass());
                            damageable.TakeDamage(mainHandDamage);
                            Debug.Log("Main Hand Hit: " + mainHandDamage + " damage");

                            // Slight delay for second hit
                            StartCoroutine(DualWieldSecondHit(damageable, baseAttack));
                        }
                        else
                        {
                            // Single weapon: One damage instance
                            float baseAttack = playerStats != null ? playerStats.GetAttackDamage() : 0f;
                            float weaponDamage = playerEquipment != null ? playerEquipment.GetWeaponDamage() : 0f;
                            WeaponClass weaponClass = playerEquipment != null ? playerEquipment.GetMainHandWeaponClass() : WeaponClass.None;

                            float totalDamage = CalculateDamageWithMastery(baseAttack, weaponDamage, weaponClass);

                            damageable.TakeDamage(totalDamage);
                            Debug.Log("Dealt " + totalDamage + " damage");
                        }

                        if (playerEquipment != null && playerBuffs != null)
                        {
                            WeaponClass weaponClass = playerEquipment.GetMainHandWeaponClass();
                            if (weaponClass == WeaponClass.Axe)
                            {
                                playerBuffs.OnAxeHit();
                            }

                            // If dual wielding, check offhand too
                            if (playerEquipment.IsDualWielding())
                            {
                                WeaponClass offHandClass = playerEquipment.GetOffHandWeaponClass();
                                if (offHandClass == WeaponClass.Axe)
                                {
                                    playerBuffs.OnAxeHit();
                                }
                            }
                        }
                    }
                }
            }
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }

        // Visual feedback
        StartCoroutine(SlashVisual());

        Debug.Log("SLASH!");
    }

    System.Collections.IEnumerator SlashVisual()
    {
        if (slashVisualObject != null)
        {
            // Orient slash toward mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 direction = (mousePos - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            slashVisualObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

            // Show it
            slashVisualObject.SetActive(true);

            // Wait
            yield return new WaitForSeconds(slashDuration);

            // Hide it
            slashVisualObject.SetActive(false);
        }

        isAttacking = false;
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }

    public float GetCooldownPercent()
    {
        if (isAttacking)
        {
            return 1f;
        }

        float timeSinceLastAttack = Time.time - lastAttackTime;

        if (timeSinceLastAttack >= attackCooldown)
        {
            return 0f;
        }
        else
        {
            return 1f - (timeSinceLastAttack / attackCooldown);
        }
    }

    System.Collections.IEnumerator DualWieldSecondHit(IDamageable damageable, float baseAttack)
    {
        yield return new WaitForSeconds(0.1f);

        if (damageable != null && playerEquipment != null)
        {
            float offHandDamage = CalculateDamageWithMastery(baseAttack, playerEquipment.GetOffHandDamage(), playerEquipment.GetOffHandWeaponClass());
            damageable.TakeDamage(offHandDamage);
            Debug.Log("Off Hand Hit: " + offHandDamage + " damage");
        }
    }

    float CalculateDamageWithMastery(float baseAttack, float weaponDamage, WeaponClass weaponClass)
    {
        float totalDamage = baseAttack + weaponDamage;

        // Apply weapon mastery bonus
        PlayerTalents talents = GetComponent<PlayerTalents>();
        if (talents != null && weaponClass != WeaponClass.None)
        {
            float masteryBonus = talents.GetWeaponMasteryDamageBonus(weaponClass);
            if (masteryBonus > 0)
            {
                float bonusDamage = totalDamage * (masteryBonus / 100f);
                totalDamage += bonusDamage;
                Debug.Log("Weapon Mastery Bonus: +" + masteryBonus + "% (+" + bonusDamage + " damage)");
            }
        }

        // Apply War Cry damage multiplier
        if (playerWarCry != null)
        {
            float warCryMultiplier = playerWarCry.GetDamageMultiplier();
            if (warCryMultiplier > 1f)
            {
                float originalDamage = totalDamage;
                totalDamage *= warCryMultiplier;
                Debug.Log("War Cry Bonus: " + (warCryMultiplier - 1f) * 100f + "% (+" + (totalDamage - originalDamage) + " damage)");
            }
        }

        return totalDamage;
    }
}