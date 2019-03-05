using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatFood : GoapAction {
    public EatFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_FOOD, actor, poiTarget) {
    }

    #region Overrides
    //protected override void ConstructRequirement() {
    //    _requirementAction = Requirement;
    //}
    protected override void ConstructPreconditionsAndEffects() {
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            if(actor.currentStructure == poiTarget.gridTileLocation.structure) {
                actor.ResetFullnessMeter();
                OnPerformActualActionToTarget();
                return true;
            }
        }
        return false;
    }
    protected override int GetCost() {
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.FOOD) {
            Food food = poiTarget as Food;
            if(food.foodType == FOOD.BERRY || food.foodType == FOOD.MUSHROOM) {
                return 3;
            } else {
                return 5;
            }
        } else if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            //TODO: 1 (actor char is owner), 5 (friend, relative, lover, paramour is owner), 10 (otherwise)
            return 10;
        }
        return 0;
    }
    #endregion

    #region Requirements
    //protected bool Requirement() {
    //    //TODO: if poitarget is Table, check for table's state if active or inactive
    //    return actor.currentStructure == poiTarget.gridTileLocation.structure;
    //}
    #endregion
}
