using UnityEngine;
using TMPro;

public class BuffUI : MonoBehaviour
{
    [SerializeField] private PlayerBuffs playerBuffs;
    [SerializeField] private TextMeshProUGUI buffText;

    void Update()
    {
        if (playerBuffs != null && buffText != null)
        {
            if (playerBuffs.HasAxeFrenzy())
            {
                int stacks = playerBuffs.GetAxeFrenzyStacks();
                float duration = playerBuffs.GetAxeFrenzyDuration();
                buffText.text = stacks + "(" + duration.ToString("F1") + "s)";
            }
            else
            {
                buffText.text = "";
            }
        }
    }
}