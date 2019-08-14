using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetFood : GoapAction {
    public int neededFood { get; private set; }
    public GetFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.GET_FOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        FoodPile foodPile = poiTarget as FoodPile;
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = foodPile.foodInPile, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            FoodPile foodPile = poiTarget as FoodPile;
            if (foodPile.foodInPile > 0) {
                SetState("Take Success");
            } else {
                SetState("Take Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return Utilities.rng.Next(10, 21);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 1 && otherData[0] is int) {
            neededFood = (int) otherData[0];
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
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
    #endregion

    #region State Effects
    private void PreTakeSuccess() {
        FoodPile foodPile = poiTarget as FoodPile;
        int takenFood = neededFood - actor.food;
        if(foodPile.foodInPile < takenFood) {
            takenFood = foodPile.foodInPile;
        }
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, takenFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterTakeSuccess() {
        FoodPile foodPile = poiTarget as FoodPile;
        int takenFood = neededFood - actor.food;
        if (foodPile.foodInPile < takenFood) {
            takenFood = foodPile.foodInPile;
        }
        actor.AdjustFood(takenFood);
        foodPile.AdjustFoodInPile(-takenFood);
    }
    //private void PreTakeFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}
