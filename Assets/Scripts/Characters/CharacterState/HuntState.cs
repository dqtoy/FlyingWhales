using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntState : CharacterState {

    public HuntState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Hunt State";
        characterState = CHARACTER_STATE.HUNT;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 96;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartHuntMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            stateComponent.character.marker.AddHostileInRange(targetPOI as Character);
            return true;
        }else if (targetPOI is Corpse) {
            //Eat action - same as explore state's pick up item action
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    #endregion

    private void StartHuntMovement() {
        stateComponent.character.marker.GoToTile(PickRandomTileToGoTo(), stateComponent.character, () => StartHuntMovement());
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
