﻿using System.Collections;
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
    #endregion

    private void StartStrollMovement() {
        stateComponent.character.marker.GoToTile(PickRandomTileToGoTo(), stateComponent.character, () => StartStrollMovement());
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