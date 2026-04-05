using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI npcNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image npcPortrait;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        HideDialogue();
    }

    /// <summary>
    /// Show dialogue with an optional portrait.
    /// Existing callers that don't pass a portrait will just hide the portrait image.
    /// </summary>
    public void ShowDialogue(string npcName, string dialogue, Sprite portrait = null)
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npcName;

        if (dialogueText != null)
            dialogueText.text = dialogue;

        // Show portrait if one is provided, hide it if not
        if (npcPortrait != null)
        {
            if (portrait != null)
            {
                npcPortrait.sprite = portrait;
                npcPortrait.gameObject.SetActive(true);
            }
            else
            {
                npcPortrait.gameObject.SetActive(false);
            }
        }
    }

    public void HideDialogue()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (npcPortrait != null)
            npcPortrait.gameObject.SetActive(false);
    }

    public bool IsDialogueShowing()
    {
        return dialoguePanel != null && dialoguePanel.activeSelf;
    }
}