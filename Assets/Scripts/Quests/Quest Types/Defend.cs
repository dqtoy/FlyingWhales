using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Defend : Quest {

	private BaseLandmark _landmarkToDefend;

	#region getters/setters
	public BaseLandmark landmarkToDefend {
		get { return _landmarkToDefend; }
	}
	#endregion

	public Defend(QuestCreator createdBy, int daysBeforeDeadline, BaseLandmark landmarkToDefend) 
        : base(createdBy, daysBeforeDeadline, QUEST_TYPE.DEFEND) {
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((MilitaryManager)createdBy).owner}),
//			new MustBeRole(CHARACTER_ROLE.WARLORD),
		};
		_landmarkToDefend = landmarkToDefend;
		_activeDuration = 30;
	}

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		goToLocation.InititalizeAction(_landmarkToDefend.location);
		goToLocation.onQuestActionDone += EndQuestAfterDays;
        goToLocation.onQuestDoAction += goToLocation.Defend;

        _questLine.Enqueue(goToLocation);
    }
    #endregion

	private void EndQuestAfterDays() {
		ScheduleQuestEnd(30, QUEST_RESULT.SUCCESS);
	}
}
