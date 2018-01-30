using UnityEngine;
using System.Collections;

public class MoveTo : CharacterTask {
    private HexTile _targetTile;
    private PATHFINDING_MODE _pathfindingMode;

    #region getters/setters
    public HexTile targetTile {
        get { return _targetTile; }
    }
    #endregion

    public MoveTo(TaskCreator createdBy, HexTile targetTile, PATHFINDING_MODE pathFindingMode) 
        : base(createdBy, TASK_TYPE.MOVE_TO) {
        _targetTile = targetTile;
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
        EndTask(TASK_RESULT.SUCCESS);
    }

    private void GoToTile() {
        GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
        goToLocation.InititalizeAction(_targetTile);
        goToLocation.SetPathfindingMode(_pathfindingMode);
        goToLocation.onTaskActionDone += SuccessTask;
        goToLocation.onTaskDoAction += goToLocation.Generic;

        goToLocation.DoAction(_assignedCharacter);
    }
}
