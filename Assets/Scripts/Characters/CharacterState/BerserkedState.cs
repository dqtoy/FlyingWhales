using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkedState : CharacterState {

    private System.Func<Character, bool> hostileChecker;
    public bool areCombatsLethal { get; private set; }

    public BerserkedState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Berserked State";
        characterState = CHARACTER_STATE.BERSERKED;
        stateCategory = CHARACTER_STATE_CATEGORY.MAJOR;
        duration = 100;
        SetAreaCombatsLethal(true);
    }

    #region Overrides
    protected override void StartState() {
        stateComponent.character.AdjustDoNotGetHungry(1);
        stateComponent.character.AdjustDoNotGetLonely(1);
        stateComponent.character.AdjustDoNotGetTired(1);
        stateComponent.character.AddTrait("Berserked");
        base.StartState();
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
            if (stateComponent.character.faction == PlayerManager.Instance.player.playerFaction) {
                return stateComponent.character.marker.AddHostileInRange(targetPOI as Character, isLethal: areCombatsLethal); //check hostility if from player faction, so as not to attack other characters that are also from the same faction.
            } else {
                if (hostileChecker != null) {
                    if (hostileChecker.Invoke(targetPOI as Character)) {
                        return stateComponent.character.marker.AddHostileInRange(targetPOI as Character, checkHostility: false, isLethal: areCombatsLethal);
                    }
                } else {
                    return stateComponent.character.marker.AddHostileInRange(targetPOI as Character, checkHostility: false, isLethal: areCombatsLethal);
                }
            }
            //return true;
        }else if (targetPOI is TileObject) {
            TileObject target = targetPOI as TileObject;
            if(target.tileObjectType != TILE_OBJECT_TYPE.TREE && target.poiGoapActions.Contains(INTERACTION_TYPE.TILE_OBJECT_DESTROY)) {
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
        for (int i = 0; i < stateComponent.character.marker.avoidInRange.Count; i++) {
            Character hostile = stateComponent.character.marker.avoidInRange[i];
            if (stateComponent.character.marker.inVisionCharacters.Contains(hostile)) {
                stateComponent.character.marker.AddHostileInRange(hostile, checkHostility: false, processCombatBehavior: false, isLethal: areCombatsLethal);
            } else {
                stateComponent.character.marker.RemoveAvoidInRange(hostile, false);
                i--;
            }
        }
        stateComponent.character.marker.ClearAvoidInRange(false);

        bool hasProcessedCombatBehavior = false;
        if (base.InVisionPOIsOnStartState()) {
            for (int i = 0; i < stateComponent.character.marker.inVisionPOIs.Count; i++) {
                IPointOfInterest poi = stateComponent.character.marker.inVisionPOIs[i];
                if (OnEnterVisionWith(poi)) {
                    if(poi is Character) {
                        hasProcessedCombatBehavior = true;
                    }
                    break;
                }
            }
        }
        if (stateComponent.character.marker.hostilesInRange.Count > 0 && !hasProcessedCombatBehavior) {
            stateComponent.SwitchToState(CHARACTER_STATE.COMBAT);
        }
        return true;
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

    public void SetHostileChecker(System.Func<Character, bool> hostileChecker) {
        this.hostileChecker = hostileChecker;
    }
    public void SetAreaCombatsLethal(bool state) {
        areCombatsLethal = state;
    }
}
