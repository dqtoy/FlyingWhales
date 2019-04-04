using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploreState : CharacterState {

    public ExploreState(CharacterStateComponent characterComp) : base (characterComp) {
        stateName = "Explore State";
        characterState = CHARACTER_STATE.EXPLORE;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartExploreMovement();
    }
    #endregion

    private void StartExploreMovement() {
        stateComponent.character.marker.GoToTile(PickRandomTileToGoTo(), stateComponent.character, () => StartExploreMovement());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomUnoccupiedTile();
        if(chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No unoccupied tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
