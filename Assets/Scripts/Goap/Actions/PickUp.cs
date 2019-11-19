using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PickUp : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PickUp() : base(INTERACTION_TYPE.PICK_UP) {
        actionIconString = GoapActionStateDB.Explore_Icon;
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.ITEM };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        //SpecialToken token = poiTarget as SpecialToken;
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = poiTarget, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), targetPOI = actor });
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_ITEM, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List <GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        SpecialToken token = target as SpecialToken;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = token.specialTokenType.ToString(), target = GOAP_EFFECT_TARGET.ACTOR });
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            SpecialToken token = poiTarget as SpecialToken;
            return poiTarget.gridTileLocation != null && actor.GetToken(token) == null;
        }
        return false;
        
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.ITEM_1);
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        goapNode.actor.PickUpToken(goapNode.poiTarget as SpecialToken);
    }
    #endregion
}

public class PickItemData : GoapActionData {
    public PickItemData() : base(INTERACTION_TYPE.PICK_UP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        SpecialToken token = poiTarget as SpecialToken;
        return poiTarget.gridTileLocation != null && actor.GetToken(token) == null;
    }
}
