using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolState : CharacterState {

    public PatrolState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Patrol State";
        characterState = CHARACTER_STATE.PATROL;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartPatrolMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
            return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    #endregion

    private void StartPatrolMovement() {
        stateComponent.character.marker.GoToTile(PickRandomTileToGoTo(), stateComponent.character, () => StartPatrolMovement());
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomUnoccupiedTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No unoccupied tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
