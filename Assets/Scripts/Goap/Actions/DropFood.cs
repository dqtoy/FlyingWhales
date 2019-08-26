using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropFood : GoapAction {
    public DropFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_FOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = 0, targetPOI = actor }, IsActorFoodEnough);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = 0, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Drop Success");
        } else {
            SetState("Target Missing");
        }
        
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Preconditions
    private bool IsActorFoodEnough() {
        if(poiTarget is Table) {
            Table table = poiTarget as Table;
            if((actor.food + table.food) >= 60) {
                return true;
            }
        } else if (poiTarget is FoodPile) {
            //FoodPile foodPile = poiTarget as FoodPile;
            if (actor.food >= 10) {
                return true;
            }
        }
        return false;
    }
    #endregion
    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
    #endregion

    #region State Effects
    private void PreDropSuccess() {
        int givenFood = actor.food;
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(null, givenFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDropSuccess() {
        int givenFood = actor.food;
        if (poiTarget is Table) {
            Table table = poiTarget as Table;
            actor.AdjustFood(-givenFood);
            table.AdjustFood(givenFood);
        } else if (poiTarget is FoodPile) {
            FoodPile foodPile = poiTarget as FoodPile;
            actor.AdjustFood(-givenFood);
            foodPile.AdjustFoodInPile(givenFood);
        }
    }
    #endregion
}

public class DropFoodData : GoapActionData {
    public DropFoodData() : base(INTERACTION_TYPE.DROP_FOOD) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        return actor.homeArea == poiTarget.gridTileLocation.structure.location;
    }
}
