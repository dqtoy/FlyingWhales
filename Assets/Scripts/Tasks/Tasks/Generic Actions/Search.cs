using UnityEngine;
using System.Collections;
using ECS;
using System;

public class Search : CharacterTask {

    private Action _searchAction;

    public Search(TaskCreator createdBy, int defaultDaysLeft, object searchingFor, ILocation targetLocation) : base(createdBy, TASK_TYPE.SEARCH, defaultDaysLeft) {
        SetStance(STANCE.NEUTRAL);
        _targetLocation = targetLocation;
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        _assignedCharacter.GoToLocation(_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
    }
    public override void PerformTask() {
        base.PerformTask();

        if (_daysLeft == 0) {
            //EndRecruitment();
            return;
        }
        ReduceDaysLeft(1);
    }
    #endregion

    private void SearchForHeirloomNecklace() {

    }
}
