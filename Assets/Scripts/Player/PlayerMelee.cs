using System.Collections;
using UnityEngine;

public class PlayerMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackArc = 90f;
    [SerializeField] private float attackCooldown = 0.5f;

    [Header("VFX")]
    [SerializeField] private Animator AttackTelegraphs;

    [Header("Hit Feel")]
    [SerializeField] private float hitStopDuration  = 0.06f;
    [SerializeField] private float hitStopTimeScale = 0.05f;

    private PlayerStats playerStats;
    private PlayerWarCry playerWarCry;
    private Animator animator;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    private Vector3 cachedAttackDirection;
    private bool hitRegistered = false;
    private Rigidbody2D rb;
    private PlayerMovement playerMovement;

    [Header("Attack Hitboxes")]
    [SerializeField] private Collider2D attackHitbox_N;
    [SerializeField] private Collider2D attackHitbox_NE;
    [SerializeField] private Collider2D attackHitbox_E;
    [SerializeField] private Collider2D attackHitbox_SE;
    [SerializeField] private Collider2D attackHitbox_S;
    [SerializeField] private Collider2D attackHitbox_SW;
    [SerializeField] private Collider2D attackHitbox_W;
    [SerializeField] private Collider2D attackHitbox_NW;

    private Collider2D activeHitbox;
    Collider2D GetDirectionalHitbox(Vector2 direction)
    {
        // Convert direction to angle in degrees (0 = right, increases counter-clockwise)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Normalize to 0-360
        if (angle < 0) angle += 360f;

        // Each direction covers a 45 degree segment
        // E=0, NE=45, N=90, NW=135, W=180, SW=225, S=270, SE=315
        if      (angle >= 337.5f || angle < 22.5f)  return attackHitbox_E;
        else if (angle >= 22.5f  && angle < 67.5f)  return attackHitbox_NE;
        else if (angle >= 67.5f  && angle < 112.5f) return attackHitbox_N;
        else if (angle >= 112.5f && angle < 157.5f) return attackHitbox_NW;
        else if (angle >= 157.5f && angle < 202.5f) return attackHitbox_W;
        else if (angle >= 202.5f && angle < 247.5f) return attackHitbox_SW;
        else if (angle >= 247.5f && angle < 292.5f) return attackHitbox_S;
        else                                         return attackHitbox_SE;
    }
    void EnableHitbox(Collider2D hitbox)
    {
        if (activeHitbox != null)
            activeHitbox.gameObject.SetActive(false);

        activeHitbox = hitbox;

        if (activeHitbox != null)
            activeHitbox.gameObject.SetActive(true);
    }

    void DisableActiveHitbox()
    {
        if (activeHitbox != null)
        {
            activeHitbox.gameObject.SetActive(false);
            activeHitbox = null;
        }
    }
    void Start()
    {
        rb             = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerStats  = GetComponent<PlayerStats>();
        playerWarCry = GetComponent<PlayerWarCry>();
        animator     = GetComponent<Animator>();
    }

    public bool CanAttack()
    {
        return !isAttacking && Time.time >= lastAttackTime + attackCooldown;
    }

    public bool IsAttacking() => isAttacking;

    public void TriggerAttack()
    {
        if (!CanAttack()) return;

        isAttacking    = true;
        hitRegistered  = false;
        lastAttackTime = Time.time;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        cachedAttackDirection = (mousePos - transform.position).normalized;

        // Enable the correct directional hitbox
        EnableHitbox(GetDirectionalHitbox(cachedAttackDirection));

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

        if (playerMovement != null) playerMovement.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;

        StartCoroutine(DelayedMovementLock());
        
    }

    [SerializeField] private float movementLockDelay = 0.1f;

    IEnumerator DelayedMovementLock()
    {
        yield return new WaitForSeconds(movementLockDelay);

        if (isAttacking)
        {
            if (rb != null) rb.linearVelocity = Vector2.zero;
            if (playerMovement != null) playerMovement.enabled = false;
        }
    }

    /// <summary>
    /// Called by Animation Event at the frame where the arc visually lands.
    /// </summary>
    public void OnAttackHit()
    {
        if (hitRegistered) return;
        hitRegistered = true;

        int enemiesHit = 0;

        if (activeHitbox != null)
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(
                activeHitbox.bounds.center,
                activeHitbox.bounds.size,
                0f
            );

            foreach (Collider2D hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;

                IDamageable damageable = hit.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(CalculateDamage());
                    enemiesHit++;
                }
            }
        }

        if (enemiesHit > 0)
            StartCoroutine(HitStop(hitStopDuration, hitStopTimeScale));

        DisableActiveHitbox();
        isAttacking = false;
    }

    /// <summary>
    /// Called by Animation Event on the last frame of the attack animation.
    /// </summary>
    public void OnAttackEnd()
    {
        DisableActiveHitbox();
        isAttacking = false;
        if (playerMovement != null)
            playerMovement.enabled = true;
    }

    float CalculateDamage()
    {
        float damage = playerStats != null ? playerStats.GetAttackDamage() : 0f;

        if (playerWarCry != null)
            damage *= playerWarCry.GetDamageMultiplier();

        return damage;
    }

    IEnumerator HitStop(float duration, float timeScale)
    {
        Time.timeScale = timeScale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
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