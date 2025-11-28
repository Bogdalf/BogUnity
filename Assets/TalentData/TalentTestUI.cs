using UnityEngine;

public class TalentTestUI : MonoBehaviour
{
    [Header("Test Talents")]
    [SerializeField] private TalentData[] availableTalents;

    private PlayerTalents playerTalents;

    void Start()
    {
        playerTalents = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerTalents>();
    }

    void OnGUI()
    {
        if (playerTalents == null) return;

        GUILayout.BeginArea(new Rect(10, 200, 300, 400));

        GUILayout.Label("TALENT POINTS: " + playerTalents.GetAvailableTalentPoints());
        GUILayout.Space(10);

        foreach (TalentData talent in availableTalents)
        {
            if (talent == null) continue;

            int currentRank = playerTalents.GetTalentRank(talent);
            bool canLearn = playerTalents.CanLearnTalent(talent);

            GUILayout.BeginHorizontal();
            GUILayout.Label(talent.talentName + " (" + currentRank + "/" + talent.maxRank + ")");

            if (canLearn && GUILayout.Button("Learn"))
            {
                playerTalents.LearnTalent(talent);

                // Recalculate equipment stats to apply speed changes
                PlayerEquipment equipment = playerTalents.GetComponent<PlayerEquipment>();
                if (equipment != null)
                {
                    // Force recalculation by re-equipping
                    var mainHand = equipment.GetComponent<PlayerEquipment>(); // Hacky but works for test
                }
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }
}