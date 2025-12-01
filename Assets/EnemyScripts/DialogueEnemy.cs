using UnityEngine;

public class DialogueEnemy : MonoBehaviour, IDamageable
{
    [Header("NPC Dialogue Settings")]
    [SerializeField] private string npcName = "Hostile Villager";

    [TextArea(3, 10)]
    [SerializeField]
    private string[] dialogueLines = new string[]
    {
        "You shouldn't have come here...",
        "This is your last warning!",
        "Now you'll pay!"
    };

    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float health = 3f;
    [SerializeField] private float damage = 1f;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 3f;

    // States
    private enum State { Passive, ShowingDialogue, Aggressive }
    private State currentState = State.Passive;

    // Dialogue tracking
    private int currentDialogueIndex = 0;
    private bool playerInRange = false;

    // Enemy behavior
    private Transform player;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Check if player is in range
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            playerInRange = distanceToPlayer <= detectionRange;

            // If player enters range while passive, start showing dialogue
            if (playerInRange && currentState == State.Passive)
            {
                currentState = State.ShowingDialogue;
                ShowNextDialogue();
            }

            // If player leaves range, hide dialogue
            if (!playerInRange && currentState == State.ShowingDialogue)
            {
                HideDialogue();
                currentState = State.Passive;
                currentDialogueIndex = 0;
            }
        }

        // Handle E key for advancing dialogue
        if (currentState == State.ShowingDialogue && Input.GetKeyDown(KeyCode.E) && playerInRange)
        {
            ShowNextDialogue();
        }
    }

    void FixedUpdate()
    {
        // Only move toward player when aggressive
        if (currentState == State.Aggressive && player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Stay still when passive or showing dialogue
            rb.linearVelocity = Vector2.zero;
        }
    }

    void ShowNextDialogue()
    {
        if (currentDialogueIndex < dialogueLines.Length)
        {
            // Show current line
            if (DialogueUI.Instance != null)
            {
                DialogueUI.Instance.ShowDialogue(npcName, dialogueLines[currentDialogueIndex]);
            }
            currentDialogueIndex++;
        }
        else
        {
            // All dialogue shown, become aggressive
            HideDialogue();
            BecomeAggressive();
        }
    }

    void HideDialogue()
    {
        if (DialogueUI.Instance != null)
        {
            DialogueUI.Instance.HideDialogue();
        }
    }

    void BecomeAggressive()
    {
        currentState = State.Aggressive;
        Debug.Log(npcName + " is now hostile!");
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;

        // If hit, immediately become aggressive (skip dialogue)
        if (currentState != State.Aggressive)
        {
            HideDialogue();
            BecomeAggressive();
        }

        // Spawn damage number
        if (DamageNumberManager.Instance != null)
        {
            DamageNumberManager.Instance.SpawnDamageNumber(transform.position, damageAmount, false);
        }

        // Visual feedback
        StartCoroutine(FlashRed());

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        HideDialogue();
        Destroy(gameObject);
    }

    System.Collections.IEnumerator FlashRed()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Color originalColor = sprite.color;
            sprite.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sprite.color = originalColor;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Only damage player when aggressive
        if (currentState == State.Aggressive && collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}