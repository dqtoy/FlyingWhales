using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Eat : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.CONSUME; } }

    public Eat() : base(INTERACTION_TYPE.EAT) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Eat Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 50;
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.AdjustDoNotGetHungry(-1);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, target, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            if (target.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Eat Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Effects
    public void PreEatSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        goapNode.actor.AdjustDoNotGetHungry(1);
        //actor.traitContainer.AddTrait(actor,"Eating");
    }
    public void PerTickEatSuccess(ActualGoapNode goapNode) {
        //goapNode.actor.AdjustFullness(520);
    }
    public void AfterEatSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetHungry(-1);
        //goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    public void PreEatFail(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreTargetMissing(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.actor.currentStructure.location, goapNode.actor.currentStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable()) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (poiTarget.gridTileLocation != null) {
                return true;
            }
        }
        return false;
    }
    #endregion
}

public class EatData : GoapActionData {
    public EatData() : base(INTERACTION_TYPE.EAT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable()) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (poiTarget.gridTileLocation != null) {
            return true;
        }
        return false;
    }
}