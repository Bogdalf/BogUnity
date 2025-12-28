using UnityEngine;
using UnityEngine.UI;

public class MeleeCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMelee playerMelee;
    [SerializeField] private Image cooldownOverlay;

    void Update()
    {
        if (playerMelee != null && cooldownOverlay != null)
        {
            UpdateCooldown();
        }
    }

    void UpdateCooldown()
    {
        float cooldownPercent = playerMelee.GetCooldownPercent();

        // Update fill amount (1 = full cooldown, 0 = ready)
        cooldownOverlay.fillAmount = cooldownPercent;

        // Optional: Change overlay color based on state
        if (cooldownPercent <= 0f)
        {
            // Ready - make overlay invisible or green tint
            cooldownOverlay.color = new Color(0f, 1f, 0f, 0.3f); // Green, transparent
        }
        else
        {
            // On cooldown - dark overlay
            cooldownOverlay.color = new Color(0f, 0f, 0f, 0.7f); // Black, semi-opaque
        }
    }
}