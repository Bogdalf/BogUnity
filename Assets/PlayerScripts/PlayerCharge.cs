using UnityEngine;

public class PlayerCharge : MonoBehaviour
{
    [Header("Charge Settings")]
    [SerializeField] private float chargeSpeed = 12f;
    [SerializeField] private float chargeDuration = 0.4f;
    [SerializeField] private float chargeCooldown = 3f;

    [Header("Telegraph Visual")]
    [SerializeField] private GameObject telegraphObject;
    [SerializeField] private float telegraphDistance = 3f;

    [Header("Aiming Settings")]
    [SerializeField] private float aimingMovementMultiplier = 0.3f;

    [Header("Damage Settings")]
    [SerializeField] private float damageMultiplier = 1.5f;

    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    private bool isCharging = false;
    private bool isAiming = false;
    private float chargeTimeLeft = 0f;
    private float lastChargeTime = -999f;
    private Vector2 chargeDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();

        if (telegraphObject != null)
        {
            telegraphObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1) && CanCharge())
        {
            StartAiming();
        }

        if (isAiming && Input.GetMouseButtonDown(1))
        {
            UpdateTelegraph();
        }

        if (isAiming && Input.GetMouseButtonUp(1))
        {
            ExecuteCharge();
        }

        if (isCharging)
        {
            chargeTimeLeft -= Time.deltaTime;

            if (chargeTimeLeft <= 0)
            {
                EndCharge();
            }
        }
    }

    void FixedUpdate()
    {
        if (isCharging)
        {
            rb.linearVelocity = chargeDirection * chargeSpeed;
        }
    }

    bool CanCharge()
    {
        return !isCharging && !isAiming && Time.time >= lastChargeTime + chargeCooldown;
    }

    void StartAiming()
    {
        isAiming = true;

        if (telegraphObject != null)
        {
            telegraphObject.SetActive(true);
        }

        Debug.Log("Started aiming charge");
    }

    void UpdateTelegraph()
    {
        if (telegraphObject == null) return;

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 direction = (mousePos - transform.position).normalized;

        // Just rotate, keep the local position it was set to in the editor
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        telegraphObject.transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        // Don't override position - let the editor value stay
    }

    void ExecuteCharge()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        chargeDirection = (mousePos - transform.position).normalized;

        isAiming = false;
        isCharging = true;
        chargeTimeLeft = chargeDuration;
        lastChargeTime = Time.time;

        if (telegraphObject != null)
        {
            telegraphObject.SetActive(false);
        }

        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        Debug.Log("CHARGE!");
    }

    void EndCharge()
    {
        isCharging = false;

        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    public bool IsCharging()
    {
        return isCharging;
    }

    public bool IsAiming()
    {
        return isAiming;
    }

    public float GetAimingMovementMultiplier()
    {
        return aimingMovementMultiplier;
    }

    public float GetCooldownPercent()
    {
        if (isCharging || isAiming)
        {
            return 1f;
        }

        float timeSinceLastCharge = Time.time - lastChargeTime;

        if (timeSinceLastCharge >= chargeCooldown)
        {
            return 0f;
        }
        else
        {
            return 1f - (timeSinceLastCharge / chargeCooldown);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isCharging) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAI enemy = collision.gameObject.GetComponent<EnemyAI>();
            if (enemy != null)
            {
                float baseDamage = playerStats != null ? playerStats.GetAttackDamage() : 2f;
                float chargeDamage = baseDamage * damageMultiplier;
                enemy.TakeDamage(chargeDamage);
                Debug.Log("Charge hit enemy for " + chargeDamage + " damage!");
            }
        }
    }
}