using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sleep : GoapAction {
    public Sleep(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 20, targetPOI = actor }, HasSupply);
        }
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            actor.ResetTirednessMeter();
            if (poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN) {
                actor.AdjustSupply(-20);
            }
            OnPerformActualActionToTarget();
            return true;
        }
        return false;
    }
    protected override int GetCost() {
        if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.STRUCTURE) {
            return 10;
        } else if (poiTarget.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN) {
                return 3;
            } else if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING) {
                if (actor.isAtHomeArea) {
                    return 1;
                } else {
                    return 5;
                }
            }
        }
        return 0;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.DWELLING && (actor.homeStructure == null || actor.homeStructure != poiTarget.gridTileLocation.structure)) {
            return false;
        }
        return true;
    }
    #endregion

    #region Preconditions
    private bool HasSupply() {
        return actor.supply >= 20;
    }
    #endregion
}
