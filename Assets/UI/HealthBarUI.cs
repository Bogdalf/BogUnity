using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private Image healthBarFill;
    [SerializeField] private TextMeshProUGUI healthText;

    void Update()
    {
        if (playerHealth != null)
        {
            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        float currentHealth = playerHealth.GetCurrentHealth();
        float maxHealth = playerHealth.GetMaxHealth();

        // Update fill amount (0 to 1)
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        // Update text
        if (healthText != null)
        {
            healthText.text = Mathf.Ceil(currentHealth) + " / " + maxHealth;
        }
    }
}