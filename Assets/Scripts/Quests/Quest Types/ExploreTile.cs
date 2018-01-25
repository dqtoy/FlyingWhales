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
        goToLandmark.onQuestDoAction += LogGoToLocation;

        //Enqueue all actions
        _questLine.Enqueue(goToLandmark);
    }
    internal override void QuestCancel() {
        _questResult = QUEST_RESULT.CANCEL;
        _assignedParty.partyLeader.DestroyAvatar();
        PartyManager.Instance.RemoveParty(_assignedParty);
        ResetQuestValues();
    }
	internal override void Result(bool isSuccess){
		if (isSuccess) {
			_landmarkToExplore.SetExploredState(true);
			EndQuest(QUEST_RESULT.SUCCESS);
			AddNewLog(_assignedParty.name + " successfully explores " + _landmarkToExplore.location.name);
		} else {
			AddNewLog("All members of " + _assignedParty.name + " died in combat, they were unable to explore the landmark.");
			QuestCancel();
		}
	}
    #endregion

    private void TriggerRandomResult() {
        StartExploration();
    }
	private void StartExploration(){
		if (_landmarkToExplore.encounterables.GetTotalOfWeights() > 0) {
			IEncounterable chosenEncounter = _landmarkToExplore.encounterables.PickRandomElementGivenWeights();
			AddNewLog("The party encounters a " + chosenEncounter.encounterName);
			chosenEncounter.StartEncounter(_assignedParty);
		}
	}
    private void ScheduleRandomResult() {
        //Once it arrives, log which Landmark is hidden in the tile.
        Log newLog = new Log(GameManager.Instance.Today(), "Quests", "ExploreTile", "discover_landmark");
        newLog.AddToFillers(_assignedParty, _assignedParty.name, LOG_IDENTIFIER.ALLIANCE_NAME);
        newLog.AddToFillers(_landmarkToExplore, Utilities.NormalizeString(_landmarkToExplore.specificLandmarkType.ToString()), LOG_IDENTIFIER.OTHER);
        UIManager.Instance.ShowNotification(newLog);
        AddNewLog("The party discovers an " + Utilities.NormalizeString(_landmarkToExplore.specificLandmarkType.ToString()));
        _landmarkToExplore.SetHiddenState(false);
        //After 5 days in the tile, the Quest triggers a random result based on data from the Landmark being explored.
        ScheduleQuestAction(5, () => TriggerRandomResult());
    }

    #region Logs
    private void LogGoToLocation() {
        AddNewLog("The party travels to " + _landmarkToExplore.location.name);
    }
    #endregion
}
