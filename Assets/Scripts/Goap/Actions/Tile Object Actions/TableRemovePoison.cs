using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableRemovePoison : GoapAction {
    protected override string failActionState { get { return "Remove Poison Fail"; } }

    public TableRemovePoison(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REMOVE_POISON_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Remove Poison";
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), targetPOI = actor }, HasItemTool);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Remove Poison Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 4;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Remove Poison Fail");
    //}
    #endregion

    #region State Effects
    public void PreRemovePoisonSuccess() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterRemovePoisonSuccess() {
        //**Effect 1**: Remove Poisoned Trait from target table
        RemoveTraitFrom(poiTarget, "Poisoned");
        //**Effect 2**: Remove Tool from Actor's inventory
        if (actor.HasTokenInInventory(SPECIAL_TOKEN.TOOL)) {
            actor.ConsumeToken(actor.GetToken(SPECIAL_TOKEN.TOOL));
        } else {
            //the actor does not have a tool, log for now
            Debug.LogWarning(actor.name + " does not have a tool for removing poison! Poison was still removed, but thought you should know.");
        }
       
    }
    #endregion

    #region Requirement
    private bool Requirement() {
        //**Advertiser**: All Tables with Poisoned trait
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        return poiTarget.GetNormalTrait("Poisoned") != null;
    }
    #endregion

    #region Preconditions
    private bool HasItemTool() {
        return actor.GetToken("Tool") != null;
    }
    #endregion
}
