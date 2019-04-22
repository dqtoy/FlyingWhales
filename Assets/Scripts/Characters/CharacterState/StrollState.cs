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
        List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentAreaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 3, false, true);
        if (tiles.Count > 0) {
            return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        } else {
            throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        }
        //int multiplier = 1;//UnityEngine.Random.Range(5, 8);
        //Vector3 forwardPos = stateComponent.character.marker.visualsParent.up * multiplier;
        //Vector2Int forwardPosInt = new Vector2Int((int) forwardPos.x, (int) forwardPos.y);
        //LocationGridTile chosenTile = stateComponent.character.GetLocationGridTileByXY(forwardPosInt.x, forwardPosInt.y, false);
        //if (chosenTile != null && chosenTile.structure != null) {
        //    return chosenTile;
        //} else {
        //    List<LocationGridTile> tiles = stateComponent.character.gridTileLocation.parentAreaMap.GetUnoccupiedTilesInRadius(stateComponent.character.gridTileLocation, 3, false, true);
        //    if (tiles.Count > 0) {
        //        return tiles[UnityEngine.Random.Range(0, tiles.Count)];
        //    } else {
        //        throw new System.Exception("No unoccupied tile in 3-tile radius for " + stateComponent.character.name + " to go to in " + stateName);
        //    }
        //}
    }
}
