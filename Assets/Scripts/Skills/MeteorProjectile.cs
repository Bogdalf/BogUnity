using System.Collections;
using UnityEngine;

/// <summary>
/// A meteor that telegraphs its landing position with a circle indicator,
/// then falls from above and deals damage to the player on impact.
///
/// Setup:
///   - Attach to a prefab with a SpriteRenderer (the meteor visual)
///   - Assign telegraphPrefab: a simple circle sprite with semi-transparent color
///   - Assign impactParticles: a ParticleSystem set to Play On Awake = false, Stop Action = Destroy
///   - The meteor spawns above the target position and falls down
///
/// Spawned by boss AI via MeteorProjectile.Spawn() static method.
/// </summary>
public class MeteorProjectile : MonoBehaviour
    {
    [Header("Debuff")]
    [SerializeField] private float debuffStackPercent = 0.20f; // 20% per stack
    [SerializeField] private float debuffDuration     = 10f; 
    
    [Header("Visuals")]
    [SerializeField] private SpriteRenderer meteorRenderer;
    [SerializeField] private TrailRenderer  trailRenderer;
    [SerializeField] private GameObject     telegraphPrefab;
    [SerializeField] private ParticleSystem impactParticles;

    [Header("Meteor Settings")]
    [SerializeField] private float telegraphDuration = 2f;
    [SerializeField] private float fallDistance      = 8f;   // How far above target it spawns
    [SerializeField] private float fallDuration      = 0.3f; // How fast it falls
    [SerializeField] private float damage            = 30f;
    [SerializeField] private float impactRadius      = 1f;

    [Header("Telegraph Visuals")]
    [SerializeField] private Color telegraphColor = new Color(1f, 0.3f, 0f, 0.4f);
    [SerializeField] private Color meteorColor    = new Color(1f, 0.5f, 0.1f, 1f);

    private Vector2 targetPosition;
    private bool hasDetonated = false;

    // ─── Static Spawn Helper ──────────────────────────────────────────────────────

    /// <summary>
    /// Spawn a meteor aimed at the given world position.
    /// Call this from boss AI scripts.
    /// </summary>
    public static MeteorProjectile Spawn(GameObject prefab, Vector2 targetPos)
    {
        if (prefab == null)
        {
            Debug.LogWarning("MeteorProjectile.Spawn: prefab is null!");
            return null;
        }

        // Start above the target
        Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y, 0f);
        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);
        MeteorProjectile meteor = obj.GetComponent<MeteorProjectile>();

        if (meteor != null)
            meteor.Initialize(targetPos);

        return meteor;
    }

    // ─── Initialization ───────────────────────────────────────────────────────────

    public void Initialize(Vector2 target)
    {
        targetPosition = target;
        transform.localScale = Vector3.one * (impactRadius * 1f);
        StartCoroutine(MeteorSequence());
    }

    // ─── Sequence ─────────────────────────────────────────────────────────────────

    IEnumerator MeteorSequence()
    {
        // ── Phase 1: Telegraph ────────────────────────────────────────────────────

        // Hide meteor visual during telegraph
        if (meteorRenderer != null) meteorRenderer.enabled = false;
        if (trailRenderer  != null) trailRenderer.enabled  = false;

        // Spawn telegraph circle at target position
        GameObject telegraph = null;
        if (telegraphPrefab != null)
        {
            telegraph = Instantiate(telegraphPrefab, new Vector3(targetPosition.x, targetPosition.y, 0f), Quaternion.identity);

            // Scale telegraph to match impact radius
            telegraph.transform.localScale = Vector3.one * (impactRadius * 2f);

            // Apply telegraph color if it has a SpriteRenderer
            SpriteRenderer telegraphSR = telegraph.GetComponent<SpriteRenderer>();
            if (telegraphSR != null)
                telegraphSR.color = telegraphColor;

            // Pulse the telegraph for visual urgency
            StartCoroutine(PulseTelegraph(telegraph, telegraphDuration));
        }

        yield return new WaitForSeconds(telegraphDuration);

        // Destroy telegraph
        if (telegraph != null)
            Destroy(telegraph);

        // ── Phase 2: Fall ─────────────────────────────────────────────────────────

        // Position meteor above target
        Vector3 startPos  = new Vector3(targetPosition.x, targetPosition.y + fallDistance, 0f);
        Vector3 endPos    = new Vector3(targetPosition.x, targetPosition.y, 0f);
        transform.position = startPos;

        // Show meteor
        if (meteorRenderer != null)
        {
            meteorRenderer.enabled = true;
            meteorRenderer.color   = meteorColor;
        }
        if (trailRenderer != null)
            trailRenderer.enabled = true;

        // Fall toward target
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t  = elapsed / fallDuration;

            // Ease in — starts slow, accelerates (like gravity)
            float easedT = t * t;
            transform.position = Vector3.Lerp(startPos, endPos, easedT);

            yield return null;
        }

        transform.position = endPos;

        // ── Phase 3: Impact ───────────────────────────────────────────────────────

        Detonate();
    }

    void Detonate()
    {
        if (hasDetonated) return;
        hasDetonated = true;

        // Hide meteor visual
        if (meteorRenderer != null) meteorRenderer.enabled = false;
        if (trailRenderer  != null) trailRenderer.enabled  = false;

        // Spawn impact particles
        if (impactParticles != null)
        {
            impactParticles.transform.SetParent(null); // Detach so it outlives the meteor
            impactParticles.Play();
        }

        // Damage player if in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, impactRadius);
        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag("Player")) continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                float finalDamage = damage * GetDamageMultiplier();
                playerHealth.TakeDamage(finalDamage);
                AddDebuffStack(); // Stack after hit so first meteor deals base damage
                Debug.Log($"Meteor hit player for {finalDamage} damage (stack {debuffStacks}).");
            }
            break;
        }

        // Destroy after particle system has time to finish
        Destroy(gameObject, 2f);
    }
    private static int    debuffStacks     = 0;
    private static float  debuffExpireTime = 0f;

    public static float GetDamageMultiplier()
    {
        // Clear stacks if debuff has expired
        if (Time.time > debuffExpireTime)
            debuffStacks = 0;

        return 1f + (debuffStacks * 0.20f);
    }

    static void AddDebuffStack()
    {
        // Clear expired stacks first
        if (Time.time > debuffExpireTime)
            debuffStacks = 0;

        debuffStacks++;
        debuffExpireTime = Time.time + 10f; // Reset duration on each hit
        Debug.Log($"Meteor debuff stack: {debuffStacks} — damage taken multiplier: {GetDamageMultiplier() * 100f}%");
    }

    public static void ResetDebuff()
    {
        debuffStacks = 0;
        debuffExpireTime = 0f;
    }
    // ─── Telegraph Pulse ──────────────────────────────────────────────────────────

    /// <summary>
    /// Pulses the telegraph circle — starts dim, gets brighter and more urgent
    /// as the meteor approaches.
    /// </summary>
    IEnumerator PulseTelegraph(GameObject telegraph, float duration)
    {
        if (telegraph == null) yield break;

        SpriteRenderer sr = telegraph.GetComponent<SpriteRenderer>();
        if (sr == null) yield break;

        float elapsed   = 0f;
        float pulseSpeed = 4f; // Pulses per second — increases over time for urgency

        while (elapsed < duration && telegraph != null)
        {
            elapsed += Time.deltaTime;

            // Progress 0→1 over the full duration
            float progress = elapsed / duration;

            // Pulse speed increases as meteor approaches
            pulseSpeed = Mathf.Lerp(2f, 8f, progress);

            // Alpha oscillates between dim and bright
            float minAlpha = Mathf.Lerp(0.1f, 0.4f, progress);
            float maxAlpha = Mathf.Lerp(0.5f, 0.9f, progress);
            float alpha    = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(elapsed * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);

            sr.color = new Color(telegraphColor.r, telegraphColor.g, telegraphColor.b, alpha);

            yield return null;
        }
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────────

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, impactRadius);
    }
}