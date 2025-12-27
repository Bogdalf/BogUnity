using UnityEngine;

public class PlayerWarCry : MonoBehaviour
{
    [Header("War Cry Settings")]
    [SerializeField] private float warCryRange = 5f;
    [SerializeField] private float stunDuration = 1.5f;
    [SerializeField] private float cooldown = 15f;

    [Header("Buff Settings")]
    [SerializeField] private float damageBuffPercent = 20f; // 20% increase
    [SerializeField] private float buffDuration = 10f;

    [Header("Visual")]
    [SerializeField] private GameObject warCryVisualPrefab; // Optional visual effect

    private float lastWarCryTime = -999f;
    private bool isBuffActive = false;
    private float buffTimeRemaining = 0f;

    void Update()
    {
        // Check centralized input manager
        if (InputManager.Instance != null && InputManager.Instance.IsPlayerInputBlocked())
        {
            return;
        }

        // Q key for War Cry
        if (Input.GetKeyDown(KeyCode.Q) && CanUseWarCry())
        {
            UseWarCry();
        }

        // Update buff timer
        if (isBuffActive)
        {
            buffTimeRemaining -= Time.deltaTime;
            if (buffTimeRemaining <= 0f)
            {
                EndBuff();
            }
        }
    }

    bool CanUseWarCry()
    {
        return Time.time >= lastWarCryTime + cooldown;
    }

    void UseWarCry()
    {
        lastWarCryTime = Time.time;

        // Apply self-buff
        StartBuff();

        // Find all enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, warCryRange);

        int enemiesStunned = 0;

        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                // Try to stun the enemy
                IStunnable stunnable = hit.GetComponent<IStunnable>();
                if (stunnable != null)
                {
                    stunnable.Stun(stunDuration);
                    enemiesStunned++;
                }
            }
        }

        // Spawn visual effect if assigned
        if (warCryVisualPrefab != null)
        {
            GameObject visual = Instantiate(warCryVisualPrefab, transform.position, Quaternion.identity);
            Destroy(visual, 2f); // Clean up after 2 seconds
        }

        Debug.Log("WAR CRY! Stunned " + enemiesStunned + " enemies. Damage buff active!");
    }

    void StartBuff()
    {
        isBuffActive = true;
        buffTimeRemaining = buffDuration;
        Debug.Log("War Cry buff started: +" + damageBuffPercent + "% damage for " + buffDuration + " seconds");
    }

    void EndBuff()
    {
        isBuffActive = false;
        buffTimeRemaining = 0f;
        Debug.Log("War Cry buff ended");
    }

    // Called by PlayerStats or combat scripts to get current damage multiplier
    public float GetDamageMultiplier()
    {
        if (isBuffActive)
        {
            return 1f + (damageBuffPercent / 100f); // 1.2 for 20% buff
        }
        return 1f;
    }

    public bool IsBuffActive()
    {
        return isBuffActive;
    }

    public float GetBuffTimeRemaining()
    {
        return buffTimeRemaining;
    }

    public float GetCooldownPercent()
    {
        float timeSinceLastUse = Time.time - lastWarCryTime;

        if (timeSinceLastUse >= cooldown)
        {
            return 0f; // Ready
        }
        else
        {
            return 1f - (timeSinceLastUse / cooldown);
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, warCryRange);
    }
}