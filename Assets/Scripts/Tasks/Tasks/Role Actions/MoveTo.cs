using UnityEngine;
using System.Collections;

public class MoveTo : CharacterTask {
    private ILocation _targetLocation;
    private PATHFINDING_MODE _pathfindingMode;

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

    public MoveTo(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.MOVE_TO) {
    }

	public void SetParameters(ILocation targetLocation, PATHFINDING_MODE pathfindingMode){
		_targetLocation = targetLocation;
		_pathfindingMode = pathfindingMode;
	}

    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
	}
    public override void PerformTask() {
		base.PerformTask();
		_assignedCharacter.SetCurrentTask(this);
		if (_assignedCharacter.party != null) {
			_assignedCharacter.party.SetCurrentTask(this);
        }
		Debug.Log(_assignedCharacter.name + " goes to " + _targetLocation.locationName);
        GoToTile();
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
    }

    private void GoToTile() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_targetLocation);
        goToLocation.SetPathfindingMode(_pathfindingMode);
        goToLocation.onTaskActionDone += SuccessTask;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }
}
