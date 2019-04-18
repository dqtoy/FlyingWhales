using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrollState : CharacterState {

    public StrollState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Stroll State";
        characterState = CHARACTER_STATE.STROLL;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 12;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartStrollMovement();
    }
    protected override void PerTickInState() {
        base.PerTickInState();
        if (!isDone) {
            stateComponent.character.CreatePersonalJobs();
        }
    }
    #endregion

    private void StartStrollMovement() {
        LocationGridTile target = PickRandomTileToGoTo();
        stateComponent.character.marker.GoTo(target, stateComponent.character, () => StartStrollMovement());
        //Debug.Log(stateComponent.character.name + " will stroll to " + target.ToString());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.structure.location.areaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 3);
        if (tiles.Count > 0) {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        } else {
            throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
