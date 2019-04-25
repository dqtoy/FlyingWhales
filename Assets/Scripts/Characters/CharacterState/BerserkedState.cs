using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkedState : CharacterState {

    public BerserkedState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Berserked State";
        characterState = CHARACTER_STATE.BERSERKED;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 24;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartBerserkedMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            stateComponent.character.marker.AddHostileInRange(targetPOI as Character, CHARACTER_STATE.NONE, false);
            return true;
        }else if (targetPOI is TileObject) {
            //TODO: has a 20% chance to Destroy items or tile objects that enters range.
            //stateComponent.character.marker.AddHostileInRange(targetPOI as Character, CHARACTER_STATE.NONE, false);
            //return true;
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    //protected override void PerTickInState() {
    //    base.PerTickInState();
    //    if (!isDone) {
    //        if(stateComponent.character.GetTrait("Injured") != null) {
    //            StopStatePerTick();
    //            OnExitThisState();
    //        }
    //    }
    //}
    #endregion

    private void StartBerserkedMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), stateComponent.character, () => StartBerserkedMovement());
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
