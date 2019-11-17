using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DropFood : GoapAction {

    public DropFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DROP_FOOD) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR }, IsActorFoodEnough);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.TARGET });
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
    private bool IsActorFoodEnough(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if(poiTarget is Table) {
            int neededFood = (int)otherData[0];
            Table table = poiTarget as Table;
            if (actor.food >= neededFood) {
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
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            return actor.homeArea == poiTarget.gridTileLocation.structure.location;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreDropSuccess(ActualGoapNode goapNode) {
        int givenFood = goapNode.actor.food;
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.AddLogFiller(null, givenFood.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterDropSuccess(ActualGoapNode goapNode) {
        int givenFood = goapNode.actor.food;
        if (goapNode.poiTarget is Table) {
            Table table = goapNode.poiTarget as Table;
            goapNode.actor.AdjustFood(-givenFood);
            table.AdjustFood(givenFood);
        } 
        //TODO: Moved to Drop Resource action
        //else if (poiTarget is FoodPile) {
        //    FoodPile foodPile = poiTarget as FoodPile;
        //    actor.AdjustFood(-givenFood);
        //    foodPile.AdjustFoodInPile(givenFood);
        //}
    }
    #endregion
}

public class DropFoodData : GoapActionData {
    public DropFoodData() : base(INTERACTION_TYPE.DROP_FOOD) {
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
