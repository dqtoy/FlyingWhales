using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Attack : Quest {

	private BaseLandmark _landmarkToAttack;

	#region getters/setters
	public BaseLandmark landmarkToAttack {
		get { return _landmarkToAttack; }
	}
	#endregion

	public Attack(QuestCreator createdBy, int daysBeforeDeadline, BaseLandmark landmarkToAttack) 
		: base(createdBy, daysBeforeDeadline, QUEST_TYPE.ATTACK) {		
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((MilitaryManager)createdBy).owner}),
			new MustBeRole(CHARACTER_ROLE.WARLORD),
		};
		_landmarkToAttack = landmarkToAttack;
		_activeDuration = 30;
	}

    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
		goToLocation.InititalizeAction(_landmarkToAttack.location);
		goToLocation.onQuestActionDone += EndQuestAfterDays;
        goToLocation.onQuestDoAction += goToLocation.Attack;

        _questLine.Enqueue(goToLocation);
    }
    #endregion

	private void EndQuestAfterDays() {
		ScheduleQuestEnd(30, QUEST_RESULT.SUCCESS);
	}
}
