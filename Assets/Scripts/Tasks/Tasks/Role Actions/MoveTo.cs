﻿using UnityEngine;
using System.Collections;

public class MoveTo : CharacterTask {
//    private PATHFINDING_MODE _pathfindingMode;

    #region getters/setters
    public HexTile targetTile {
        get {
            if (_targetLocation is BaseLandmark) {
                return (_targetLocation as BaseLandmark).location;
            } else {
                return (_targetLocation as HexTile);
            }
        }
    }
    #endregion

	public MoveTo(TaskCreator createdBy, int defaultDaysLeft = -1) 
        : base(createdBy, TASK_TYPE.MOVE_TO, defaultDaysLeft) {
		_forPlayerOnly = true;
		SetStance(STANCE.NEUTRAL);
    }

    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		Debug.Log(_assignedCharacter.name + " goes to " + _targetLocation.locationName);
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP);
	}
    public override void PerformTask() {
		base.PerformTask();
		SuccessTask ();
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
		DoNothing doNothing = new DoNothing (_assignedCharacter);
		doNothing.SetDaysLeft (3);
		doNothing.OnChooseTask (_assignedCharacter);
    }
}
