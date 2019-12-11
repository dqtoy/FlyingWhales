using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
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
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.ITEM };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
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
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        object[] otherData = node.otherData;
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
        if (otherData.Length >= 2) {
            SPECIAL_TOKEN tokenType = (SPECIAL_TOKEN)otherData[1];
            return actor.GetToken(tokenType) != null;
        } else {
            SpecialToken specialToken = poiTarget as SpecialToken;
            return actor.GetToken(specialToken) != null;
        }
        
    }
    #endregion

    #region State Effects
    public void PreDropSuccess(ActualGoapNode goapNode) {
        //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterDropSuccess(ActualGoapNode goapNode) {
        LocationGridTile tile = goapNode.actor.gridTileLocation.GetNearestUnoccupiedTileFromThis();
        SpecialToken specialToken;
        if (goapNode.otherData.Length >= 2) {
            SPECIAL_TOKEN tokenType = (SPECIAL_TOKEN)goapNode.otherData[1];
            specialToken = goapNode.actor.GetToken(tokenType);
        } else {
            specialToken = goapNode.poiTarget as SpecialToken;
        }

        goapNode.actor.DropToken(specialToken, goapNode.actor.gridTileLocation.structure.location, goapNode.actor.gridTileLocation.structure, tile);
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
