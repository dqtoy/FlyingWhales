using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatFood : GoapAction {
    public EatFood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_FOOD, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.FOOD) {
                if (actor.currentStructure == poiTarget.gridTileLocation.structure) {
                    actor.ResetFullnessMeter();
                    actor.currentStructure.RemovePOI(poiTarget);
                    return true;
                }
            }else if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
                if (actor.currentStructure == poiTarget.gridTileLocation.structure) {
                    actor.ResetFullnessMeter();
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        //TODO: if poitarget is Table, check for table's state if active or inactive
        return poiTarget.gridTileLocation != null;
    }
    #endregion
}
