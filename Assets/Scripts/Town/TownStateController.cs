using UnityEngine;

public class TownStateController : MonoBehaviour
{
    [Header("Gate Objects")]
    [SerializeField] private GameObject closedGate;
    [SerializeField] private GameObject openGate;

    [Header("King")]
    [SerializeField] private GameObject kingAtGateObject;

    void Start()
    {
        ApplyState();
    }

    public void ApplyState()
    {
        if (GameStateManager.Instance == null) return;

        bool metKing = GameStateManager.Instance.GetQuestFlag("MetKingInInn");

        closedGate.SetActive(!metKing);
        openGate.SetActive(metKing);
        kingAtGateObject.SetActive(metKing);
    }

    public void OnKingInnDialogueComplete()
    {
        GameStateManager.Instance.SetQuestFlag("MetKingInInn", true);
        ApplyState();
    }
}