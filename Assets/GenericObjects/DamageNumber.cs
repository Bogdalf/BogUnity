using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float lifetime = 1f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float fadeSpeed = 1f;

    private TextMeshPro textMesh;
    private Color startColor;
    private float timer = 0f;

    void Start()
    {
        textMesh = GetComponent<TextMeshPro>();
        if (textMesh != null)
        {
            startColor = textMesh.color;
        }
    }

    void Update()
    {
        // Move upward
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Fade out
        timer += Time.deltaTime;
        if (textMesh != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, timer / lifetime);
            textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
        }

        // Destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public void SetDamage(float damage, bool isPlayerDamage = false)
    {
        if (textMesh == null)
        {
            textMesh = GetComponent<TextMeshPro>();
        }

        if (textMesh != null)
        {
            textMesh.text = Mathf.Ceil(damage).ToString();
            Debug.Log("Setting damage number to: " + Mathf.Ceil(damage));

            // Different color for player taking damage
            if (isPlayerDamage)
            {
                textMesh.color = Color.yellow;
                startColor = Color.yellow;
            }
            else
            {
                textMesh.color = Color.red;
                startColor = Color.red;
            }
        }
        else
        {
            Debug.LogError("TextMeshPro component not found!");
        }
    }
}