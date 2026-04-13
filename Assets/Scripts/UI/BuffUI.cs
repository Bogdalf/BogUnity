using UnityEngine;
using TMPro;

/// <summary>
/// Displays active player buffs in the UI.
/// Currently a placeholder — hook active buffs in here when the buff system is built out.
/// </summary>
public class BuffUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI buffText;

    void Update()
    {
        if (buffText != null)
            buffText.text = "";
    }
}