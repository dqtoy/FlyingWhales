using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkedState : CharacterState {

    public BerserkedState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Berserked State";
        characterState = CHARACTER_STATE.BERSERKED;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 100;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartBerserkedMovement();
    }
    public override bool OnEnterVisionWith(IPointOfInterest targetPOI) {
        if(targetPOI is Character) {
            return stateComponent.character.marker.AddHostileInRange(targetPOI as Character, CHARACTER_STATE.NONE, false);
            //return true;
        }else if (targetPOI is TileObject) {
            TileObject target = targetPOI as TileObject;
            if(target.tileObjectType != TILE_OBJECT_TYPE.TREE) {
                //TODO: has a 20% chance to Destroy items or tile objects that enters range.
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 20) {
                    GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.TILE_OBJECT_DESTROY, stateComponent.character, targetPOI);
                    if (goapAction.targetTile != null) {
                        goapAction.CreateStates();
                        stateComponent.character.SetCurrentAction(goapAction);
                        stateComponent.character.marker.GoTo(goapAction.targetTile, targetPOI, () => OnArriveAtLocation());
                        PauseState();
                    } else {
                        Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't destroy tile object " + targetPOI.name + " because there is no tile to go to!");
                    }
                    return true;
                }
            }
        } else if (targetPOI is SpecialToken) {
            //TODO: has a 20% chance to Destroy items or tile objects that enters range.
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ITEM_DESTROY, stateComponent.character, targetPOI);
                if (goapAction.targetTile != null) {
                    goapAction.CreateStates();
                    stateComponent.character.SetCurrentAction(goapAction);
                    stateComponent.character.marker.GoTo(goapAction.targetTile, targetPOI, () => OnArriveAtLocation());
                    PauseState();
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't destroy item " + targetPOI.name + " because there is no tile to go to!");
                }
                return true;
            }
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

    private void OnArriveAtLocation() {
        stateComponent.character.currentAction.SetEndAction(BerserkAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void BerserkAgain(string result, GoapAction goapAction) {
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }
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
