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

    public MoveTo(TaskCreator createdBy, ILocation targetLocation, PATHFINDING_MODE pathFindingMode) 
        : base(createdBy, TASK_TYPE.MOVE_TO) {
        _targetLocation = targetLocation;
        _pathfindingMode = pathFindingMode;
    }

    #region overrides
    public override void PerformTask(ECS.Character character) {
        base.PerformTask(character);
        character.SetCurrentTask(this);
        if (character.party != null) {
            character.party.SetCurrentTask(this);
        }

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
