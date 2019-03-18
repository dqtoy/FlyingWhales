using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItemGoap : GoapAction {
    public PickItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PICK_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetStructure  == actor.gridTileLocation.structure) {
            SetState("Take Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return !actor.isHoldingItem && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess() {
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTakeSuccess() {
        actor.PickUpToken(poiTarget as SpecialToken);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as SpecialToken, poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
