using System.Collections;
using UnityEngine;

/// <summary>
/// A spirit that walks toward its assigned statue.
/// Stopped by player collision — plays Die animation, then fades out.
/// If it reaches the statue, the statue becomes empowered.
///
/// Each spirit has a pre-assigned SpiritStatue destination set in the Inspector
/// (or assigned by SpiritWave at spawn time).
/// </summary>
public class SpiritAI : MonoBehaviour
{
    [Header("Destination")]
    [SerializeField] private SpiritStatue targetStatue;

    [Header("Movement")]
    [SerializeField] private float moveSpeed    = 1.5f;
    [SerializeField] private float arrivalRange = 0.3f; // How close counts as "reached"

    [Header("Fade Settings")]
    [SerializeField] private float dieAnimDuration = 2f;   // Wait before starting fade
    [SerializeField] private float fadeDuration     = 1.5f; // How long the fade takes

    private Animator         animator;
    private SpriteRenderer   spriteRenderer;
    private Rigidbody2D      rb;
    private Collider2D       col;

    private bool isStopped  = false;
    private bool isDying    = false;
    private bool hasArrived = false;

    // Called by SpiritWave when this spirit resolves (stopped or arrived)
    public System.Action<SpiritAI> OnResolved;

    void Awake()
    {
        animator       = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb             = GetComponent<Rigidbody2D>();
        col            = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isStopped || isDying || hasArrived) return;
        if (targetStatue == null) return;

        MoveTowardStatue();
    }

    // ─── Setup ────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Assign the target statue — called by SpiritWave at spawn time.
    /// </summary>
    public void SetTarget(SpiritStatue statue)
    {
        targetStatue = statue;
    }

    // ─── Movement ─────────────────────────────────────────────────────────────────

    void MoveTowardStatue()
    {
        Vector2 direction = (targetStatue.transform.position - transform.position).normalized;
        float distance    = Vector2.Distance(transform.position, targetStatue.transform.position);

        if (animator != null)
        {
            animator.SetFloat("MovementX", direction.x);
            animator.SetFloat("MovementY", direction.y);
        }

        transform.position = Vector2.MoveTowards(
            transform.position,
            targetStatue.transform.position,
            moveSpeed * Time.deltaTime
        );

        if (distance <= arrivalRange)
            OnReachedStatue();
    }

    // ─── Arrival ──────────────────────────────────────────────────────────────────

    void OnReachedStatue()
    {
        if (hasArrived) return;
        hasArrived = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Empower the statue
        targetStatue?.Empower();

        Debug.Log($"Spirit reached statue: {targetStatue?.gameObject.name}");

        // Notify wave manager
        OnResolved?.Invoke(this);

        // Fade out silently
        StartCoroutine(FadeOut(0f));
    }

    // ─── Player Collision ─────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDying || isStopped || hasArrived) return;
        if (!other.CompareTag("Player")) return;

        Stop();
    }

    public void Stop()
    {
        if (isStopped || isDying || hasArrived) return;

        isStopped = true;
        isDying   = true;

        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Disable collider so player doesn't keep triggering it
        if (col != null) col.enabled = false;

        // Stop movement animator params
        if (animator != null)
        {
            animator.SetFloat("MovementX", 0f);
            animator.SetFloat("MovementY", 0f);
            animator.SetTrigger("Die");
        }

        Debug.Log("Spirit stopped by player!");

        // Notify wave manager
        OnResolved?.Invoke(this);

        // Wait for Die animation, then fade
        StartCoroutine(FadeOut(dieAnimDuration));
    }

    // ─── Fade Out ─────────────────────────────────────────────────────────────────

    IEnumerator FadeOut(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (spriteRenderer == null)
        {
            Destroy(gameObject);
            yield break;
        }

        float elapsed = 0f;
        Color startColor = spriteRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(gameObject);
    }

    // ─── Getters ──────────────────────────────────────────────────────────────────

    public bool IsStopped()  => isStopped;
    public bool HasArrived() => hasArrived;
}