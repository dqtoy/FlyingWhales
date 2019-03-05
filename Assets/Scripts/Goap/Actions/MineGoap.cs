using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineGoap : GoapAction {
    private const int MAX_SUPPLY = 50;
    private const int MIN_SUPPLY = 20;
    public MineGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MINE_ACTION, actor, poiTarget) {
    }

    #region Overrides
    //protected override void ConstructRequirement() {
    //    _requirementAction = Requirement;
    //}
    protected override void ConstructPreconditionsAndEffects() {
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = MAX_SUPPLY, targetPOI = actor });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            if (poiTarget is Ore) {
                Ore ore = poiTarget as Ore;
                int gained = ore.GetSupplyPerMine();
                actor.AdjustSupply(gained);
                ore.AdjustYield(gained);
                OnPerformActualActionToTarget();
            }
            return true;
        }
        return false;
    }
    protected override int GetCost() {
        return 3;
    }
    #endregion

    //#region Requirements
    //protected bool Requirement() {
    //    return !actor.isHoldingItem;
    //}
    //#endregion
}
