using UnityEngine;

public class AttackTelegraphs : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Start invisible
        spriteRenderer.enabled = false;
    }

    // Called by Animation Event on first frame
    public void EnableSprite()
    {
        spriteRenderer.enabled = true;
    }

    // Called by Animation Event on last frame
    public void DisableSprite()
    {
        spriteRenderer.enabled = false;
    }
}