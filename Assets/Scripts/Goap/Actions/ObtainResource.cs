using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ObtainResource : GoapAction {
    public ObtainResource() : base(INTERACTION_TYPE.OBTAIN_RESOURCE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return Utilities.rng.Next(10, 21);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest target, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, target, otherData);
        if (goapActionInvalidity.isInvalid == false) {
             FoodPile foodPile = target as FoodPile;
            if (foodPile.foodInPile <= 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.logKey = "take fail_description";
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
                if (foodPile.foodInPile > 0) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreTakeSuccess(ActualGoapNode goapNode) {
        FoodPile foodPile = goapNode.poiTarget as FoodPile;
        int neededFood = (int)goapNode.otherData[0];
        int takenFood = neededFood - goapNode.actor.food;
        if(foodPile.foodInPile < takenFood) {
            takenFood = foodPile.foodInPile;
        }
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.AddLogFiller(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, takenFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTakeSuccess(ActualGoapNode goapNode) {
        FoodPile foodPile = goapNode.poiTarget as FoodPile;
        int neededFood = (int)goapNode.otherData[0];
        int takenFood = neededFood - goapNode.actor.food;
        if (foodPile.foodInPile < takenFood) {
            takenFood = foodPile.foodInPile;
        }
        goapNode.actor.AdjustFood(takenFood);
        foodPile.AdjustFoodInPile(-takenFood);
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
            if (foodPile.foodInPile > 0) {
                return true;
            }
        }
        return false;
    }
}
