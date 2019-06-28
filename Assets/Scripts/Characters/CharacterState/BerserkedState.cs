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
    protected override void StartState() {
        base.StartState();
        stateComponent.character.AdjustDoNotGetHungry(1);
        stateComponent.character.AdjustDoNotGetLonely(1);
        stateComponent.character.AdjustDoNotGetTired(1);
        stateComponent.character.AddTrait("Berserked");
    }
    protected override void EndState() {
        base.EndState();
        stateComponent.character.AdjustDoNotGetHungry(-1);
        stateComponent.character.AdjustDoNotGetLonely(-1);
        stateComponent.character.AdjustDoNotGetTired(-1);
        stateComponent.character.AdjustHappiness(50);
        stateComponent.character.AdjustTiredness(50);
        stateComponent.character.RemoveTrait("Berserked");
    }
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
                        SetCurrentlyDoingAction(goapAction);
                        goapAction.CreateStates();
                        stateComponent.character.SetCurrentAction(goapAction);
                        stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtLocation);
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
                    stateComponent.character.marker.GoTo(goapAction.targetTile, OnArriveAtLocation);
                    PauseState();
                } else {
                    Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + stateComponent.character.name + " can't destroy item " + targetPOI.name + " because there is no tile to go to!");
                }
                return true;
            }
        }
        return base.OnEnterVisionWith(targetPOI);
    }
    public override bool InVisionPOIsOnStartState() {
        if (base.InVisionPOIsOnStartState()) {
            for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
                if (OnEnterVisionWith(poi)) {
                    return true;
                }
            }
        }
        return false;
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
    public override void AfterExitingState() {
        base.AfterExitingState();
        Spooked spooked = stateComponent.character.GetNormalTrait("Spooked") as Spooked;
        if (spooked != null) {
            //If has spooked, add them in avoid list and transfer all in engage list to flee list
            stateComponent.character.marker.AddAvoidsInRange(spooked.terrifyingCharacters, false);
            Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, stateComponent.character);
        }
    }
    #endregion

    private void OnArriveAtLocation() {
        if (stateComponent.character.currentAction == null) {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + stateComponent.character.name + " arrived at location of item/tile object to be destroyed during " + stateName + ", but current action is null");
            return;
        }
        stateComponent.character.currentAction.SetEndAction(BerserkAgain);
        stateComponent.character.currentAction.PerformActualAction();
    }
    private void BerserkAgain(string result, GoapAction goapAction) {
        SetCurrentlyDoingAction(null);
        if (stateComponent.currentState != this) {
            return;
        }
        stateComponent.character.SetCurrentAction(null);
        ResumeState();
    }
    private void StartBerserkedMovement() {
        stateComponent.character.marker.GoTo(PickRandomTileToGoTo(), StartBerserkedMovement);
    }
    private LocationGridTile PickRandomTileToGoTo() {
        LocationStructure chosenStructure = stateComponent.character.specificLocation.GetRandomStructure();
        LocationGridTile chosenTile = chosenStructure.GetRandomTile();
        if (chosenTile != null) {
            return chosenTile;
        } else {
            throw new System.Exception("No tile in " + chosenStructure.name + " for " + stateComponent.character.name + " to go to in " + stateName);
        }
    }
}
