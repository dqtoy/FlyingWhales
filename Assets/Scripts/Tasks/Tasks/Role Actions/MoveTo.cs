using UnityEngine;
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

    public MoveTo(TaskCreator createdBy) 
        : base(createdBy, TASK_TYPE.MOVE_TO) {
		_forPlayerOnly = true;
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
		_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.NORMAL, () => SuccessTask ());
    }
    #endregion

    private void SuccessTask() {
        EndTask(TASK_STATUS.SUCCESS);
        _assignedCharacter.DestroyAvatar();
		DoNothing doNothing = new DoNothing (_assignedCharacter);
		doNothing.SetDays (3);
		doNothing.OnChooseTask (_assignedCharacter);
		doNothing.PerformTask ();
    }
}
