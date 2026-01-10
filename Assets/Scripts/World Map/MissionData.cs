using UnityEngine;

[CreateAssetMenu(fileName = "New Mission", menuName = "World Map/Mission Data")]
public class MissionData : ScriptableObject
{
    [Header("Mission Info")]
    public string missionName = "Unnamed Mission";
    [TextArea(3, 6)]
    public string description = "Mission description goes here.";

    [Header("Mission Properties")]
    public int recommendedLevel = 1;
    public MissionType missionType = MissionType.Story;

    [Header("Requirements")]
    public bool isLocked = false;
    public string unlockRequirement = ""; // Description of what unlocks this

    [Header("Rewards")]
    [TextArea(2, 4)]
    public string rewardDescription = "";
    public int experienceReward = 100;
    public int goldReward = 50;

    [Header("Scene")]
    public string sceneName = ""; // Scene to load when mission starts

    public enum MissionType
    {
        Story,
        SideMission,
        ResourceGathering,
        FactionQuest,
        Boss
    }
}