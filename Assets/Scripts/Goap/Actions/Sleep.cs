using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleep : GoapAction {
    public Sleep(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetStructure == actor.gridTileLocation.structure) {
            if (poiTarget.state != POI_STATE.INACTIVE) {
                SetState("Rest Success");
            } else {
                SetState("Rest Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        Dwelling dwelling = targetStructure as Dwelling;
        if (dwelling.residents.Contains(actor)) {
            return 1;
        } else {
            for (int i = 0; i < dwelling.residents.Count; i++) {
                Character resident = dwelling.residents[i];
                if(resident != actor) {
                    CharacterRelationshipData characterRelationshipData = actor.GetCharacterRelationshipData(resident);
                    if (characterRelationshipData != null) {
                        if (characterRelationshipData.HasRelationshipOfEffect(TRAIT_EFFECT.POSITIVE)) {
                            return 4;
                        }
                    }
                }
            }
            return 10;
        }
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Rest Fail");
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(targetStructure.structureType == STRUCTURE_TYPE.DWELLING && poiTarget.state == POI_STATE.ACTIVE) {
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetTired(1);
        //actor.AddTrait("Resting");
    }
    private void PerTickRestSuccess() {
        actor.AdjustTiredness(7);
    }
    private void AfterRestSuccess() {
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
        actor.AdjustDoNotGetTired(-1);
    }
    private void PreRestFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    //private void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}
