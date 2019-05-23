using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleep : GoapAction {
    protected override string failActionState { get { return "Rest Fail"; } }

    public Sleep(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Rest Success");
        } else {
            TileObject obj = poiTarget as TileObject;
            if (!obj.IsAvailable()) {
                SetState("Rest Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = targetStructure as Dwelling;
            if (dwelling.IsResident(actor)) {
                return 1;
            } else {
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if (resident != actor) {
                        CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(resident);
                        if (characterRelationshipData != null) {
                            if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                                return 15;
                            }
                        }
                    }
                }
                return 30;
            }
        } else if (targetStructure.structureType == STRUCTURE_TYPE.INN) {
            return 30;
        }
        return 100;
    }
    //public override void FailAction() {
    //    Debug.LogError(actor.name + " failed " + goapName + " action from recalculate path!");
    //    base.FailAction();
    //    SetState("Rest Fail");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Rest Success") {
            RemoveTraitFrom(actor, "Resting");
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        IAwareness awareness = actor.GetAwareness(poiTarget);
        if (awareness == null) {
            return false;
        }
        LocationGridTile knownLoc = awareness.knownGridLocation;
        //if (targetStructure.structureType == STRUCTURE_TYPE.DWELLING && knownLoc != null) {
        //    TileObject obj = poiTarget as TileObject;
        //    return obj.IsAvailable();
        //    //if(knownLoc.occupant == null) {
        //    //    return true;
        //    //} else if (knownLoc.occupant == actor) {
        //    //    return true;
        //    //}
        //}
        //return false;
        return knownLoc != null && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //poiTarget.SetPOIState(POI_STATE.INACTIVE);
        //actor.AdjustDoNotGetTired(1);
        Resting restingTrait = new Resting();
        actor.AddTrait(restingTrait);
    }
    private void PerTickRestSuccess() {
        actor.AdjustTiredness(7);
    }
    private void AfterRestSuccess() {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        //actor.AdjustDoNotGetTired(-1);
        RemoveTraitFrom(actor, "Resting");
    }
    private void PreRestFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    //private void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}
