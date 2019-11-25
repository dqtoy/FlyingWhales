using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ObtainResource : GoapAction {
    public ObtainResource() : base(INTERACTION_TYPE.OBTAIN_RESOURCE) {
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
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, object[] otherData) {
        List<GoapEffect> ee = base.GetExpectedEffects(actor, target, otherData);
        ResourcePile pile = target as ResourcePile;
        switch (pile.providedResource) {
            case RESOURCE.FOOD:
                ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
                break;
            case RESOURCE.WOOD:
                ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_WOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
                break;
        }
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(10, 21);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            FoodPile foodPile = poiTarget as FoodPile;
            if (foodPile.resourceInPile <= 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Take Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget is FoodPile) {
                FoodPile foodPile = poiTarget as FoodPile;
                if (foodPile.resourceInPile > 0) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        FoodPile foodPile = goapNode.poiTarget as FoodPile;
        int neededFood = (int)goapNode.otherData[0];
        int takenFood = neededFood - goapNode.actor.food;
        if(foodPile.resourceInPile < takenFood) {
            takenFood = foodPile.resourceInPile;
        }
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.descriptionLog.AddToFillers(null, takenFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        FoodPile foodPile = goapNode.poiTarget as FoodPile;
        int neededFood = (int)goapNode.otherData[0];
        int takenFood = neededFood - goapNode.actor.food;
        if (foodPile.resourceInPile < takenFood) {
            takenFood = foodPile.resourceInPile;
        }
        goapNode.actor.AdjustFood(takenFood);
        foodPile.AdjustResourceInPile(-takenFood);
    }
    #endregion
}

public class GetFoodData : GoapActionData {
    public GetFoodData() : base(INTERACTION_TYPE.OBTAIN_RESOURCE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget is FoodPile) {
            FoodPile foodPile = poiTarget as FoodPile;
            if (foodPile.resourceInPile > 0) {
                return true;
            }
        }
        return false;
    }
}
