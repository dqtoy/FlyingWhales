using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ChopWood : GoapAction {
    private const int MAX_SUPPLY = 50;

    public ChopWood() : base(INTERACTION_TYPE.CHOP_WOOD) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, conditionKey = MAX_SUPPLY.ToString(), isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Chop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.GetNormalTrait("Logger") != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreChopSuccess(ActualGoapNode goapNode) {
        TreeObject tree = goapNode.poiTarget as TreeObject;
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(null, tree.GetSupplyPerMine().ToString(), LOG_IDENTIFIER.STRING_1);
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterChopSuccess(ActualGoapNode goapNode) {
        TreeObject tree = goapNode.poiTarget as TreeObject;
        goapNode.actor.AdjustSupply(tree.GetSupplyPerMine());
        tree.AdjustYield(-1);
    }
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing(ActualGoapNode goapNode) {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class ChopWoodData : GoapActionData {
    public ChopWoodData() : base(INTERACTION_TYPE.CHOP_WOOD) {
        //racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}