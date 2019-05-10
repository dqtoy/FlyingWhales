using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItemGoap : GoapAction {
    public PickItemGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PICK_ITEM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Explore_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        SpecialToken token = poiTarget as SpecialToken;
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Take Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 2;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        SpecialToken token = poiTarget as SpecialToken;
        //if(token.characterOwner == null) {
        //    if(token.factionOwner != null && actor.faction.id != token.factionOwner.id) {
        //        return false;
        //    }
        //    return poiTarget.gridTileLocation != null && actor.GetToken(token) == null;
        //}
        //return false;
        return poiTarget.gridTileLocation != null && actor.GetToken(token) == null;
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
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
