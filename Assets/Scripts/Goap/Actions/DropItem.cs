using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DropItem : GoapAction {

    public override ACTION_CATEGORY actionCategory {
        get { return ACTION_CATEGORY.DIRECT; }
    }

    public DropItem() : base(INTERACTION_TYPE.DROP_ITEM) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, target = GOAP_EFFECT_TARGET.ACTOR }, IsItemInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_ITEM, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override LocationStructure GetTargetStructure(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return otherData[0] as LocationStructure;
    }
    #endregion

    #region Requirements
    //protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
    //    bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
    //    return actor.homeStructure != null;
    //}
    #endregion

    #region Preconditions
    private bool IsItemInInventory(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        SpecialToken token = poiTarget as SpecialToken;
        return actor.GetToken(token) != null;
    }
    #endregion

    #region State Effects
    public void PreDropSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.AddLogFiller(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        currentState.AddLogFiller(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        LocationGridTile tile = goapNode.actor.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        goapNode.actor.DropToken(goapNode.poiTarget as SpecialToken, goapNode.actor.gridTileLocation.structure.location, goapNode.actor.gridTileLocation.structure, tile);
    }
    #endregion
}

public class DropItemHomeData : GoapActionData {
    public DropItemHomeData() : base(INTERACTION_TYPE.DROP_ITEM) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.homeStructure != null;
    }
}
