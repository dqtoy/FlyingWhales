using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploreTile : Quest {

    private BaseLandmark _landmarkToExplore;

    #region getters/setters
    public BaseLandmark landmarkToExplore {
        get { return _landmarkToExplore; }
    }
    #endregion
    public ExploreTile(QuestCreator createdBy, int daysBeforeDeadline, BaseLandmark landmarkToExplore) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.EXPLORE_TILE) {
        //_questFilters = new List<QuestFilter>() {
        //    new MustBeRole(CHARACTER_ROLE.CHIEFTAIN),
        //    new MustBeRole(CHARACTER_ROLE.CHIEFTAIN)
        //};
        _landmarkToExplore = landmarkToExplore;
    }

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLandmark = new GoToLocation(this); //Go to the picked region
        goToLandmark.InititalizeAction(_landmarkToExplore.location);
        goToLandmark.onQuestActionDone += ScheduleRandomResult;
        goToLandmark.onQuestDoAction += goToLandmark.Generic;

        //Enqueue all actions
        _questLine.Enqueue(goToLandmark);
    }
    #endregion

    private void TriggerRandomResult() {
        //TODO: Trigger a random result
        EndQuest(QUEST_RESULT.SUCCESS);
    }

    private void ExplorationResults() {

    }

    private void ScheduleRandomResult() {
        //Once it arrives, log which Landmark is hidden in the tile.
        Log newLog = new Log(GameManager.Instance.Today(), "Quests", "ExploreTile", "discover_landmark");
        newLog.AddToFillers(_assignedParty, _assignedParty.name, LOG_IDENTIFIER.ALLIANCE_NAME);
        newLog.AddToFillers(_landmarkToExplore, Utilities.NormalizeString(_landmarkToExplore.specificLandmarkType.ToString()), LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);
        //After 5 days in the tile, the Quest triggers a random result based on data from the Landmark being explored.
        ScheduleQuestAction(5, () => TriggerRandomResult());
    }
}
