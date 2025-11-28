using UnityEngine;
using UnityEngine.UI;

public class ChargeCooldownUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCharge playerCharge;
    [SerializeField] private Image cooldownOverlay;

    void Update()
    {
        if (playerCharge != null && cooldownOverlay != null)
        {
            UpdateCooldown();
        }
    }

    void UpdateCooldown()
    {
        float cooldownPercent = playerCharge.GetCooldownPercent();

        cooldownOverlay.fillAmount = cooldownPercent;

        if (cooldownPercent <= 0f)
        {
            cooldownOverlay.color = new Color(0f, 1f, 0f, 0.3f); // Green, ready
        }
        else
        {
            cooldownOverlay.color = new Color(0f, 0f, 0f, 0.7f); // Black, on cooldown
        }
    }
}