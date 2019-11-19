using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DropResource : GoapAction {
    public DropResource() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_FOOD, GOAP_EFFECT_TARGET.ACTOR));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_WOOD, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(IPointOfInterest target, object[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(target, otherData);
        ResourcePile pile = target as ResourcePile;
        switch (pile.providedResource) {
            case RESOURCE.FOOD:
                ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR));
                break;
            case RESOURCE.WOOD:
                ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR));
                break;
        }
        return ee;
    }
    public override List<Precondition> GetPreconditions(IPointOfInterest target, object[] otherData) {
        List<Precondition> p = base.GetPreconditions(target, otherData);
        ResourcePile pile = target as ResourcePile;
        switch (pile.providedResource) {
            case RESOURCE.FOOD:
                p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_FOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorFoodEnough));
                break;
            case RESOURCE.WOOD:
                p.Add(new Precondition(new GoapEffect(GOAP_EFFECT_CONDITION.HAS_WOOD, "0", true, GOAP_EFFECT_TARGET.ACTOR), IsActorWoodEnough));
                break;
        }
        return p;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Drop Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    #endregion

    #region Preconditions
    private bool IsActorWoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor.supply > actor.role.reservedSupply) {
            SupplyPile supplyPile = poiTarget as SupplyPile;
            int supplyToBeDeposited = actor.supply - actor.role.reservedSupply;
            if((supplyToBeDeposited + supplyPile.resourceInPile) >= 100) {
                return true;
            }
        }
        return false;
    }
    private bool IsActorFoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.food > 0;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {

        }
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
    #endregion

    #region State Effects
    private void PreDropSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        int givenSupply = goapNode.actor.supply - goapNode.actor.role.reservedSupply;
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(null, givenSupply.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDropSuccess(ActualGoapNode goapNode) {
        SupplyPile supplyPile = goapNode.poiTarget as SupplyPile;
        int givenSupply = goapNode.actor.supply - goapNode.actor.role.reservedSupply;
        goapNode.actor.AdjustSupply(-givenSupply);
        supplyPile.AdjustResourceInPile(givenSupply);
    }
    #endregion
}

public class DropResourceData : GoapActionData {
    public DropResourceData() : base(INTERACTION_TYPE.DROP_RESOURCE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
}
