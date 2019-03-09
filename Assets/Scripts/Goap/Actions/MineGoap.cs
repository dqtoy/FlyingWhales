using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineGoap : GoapAction {
    private const int MAX_SUPPLY = 50;
    private const int MIN_SUPPLY = 20;


    private int _gainedSupply;
    public MineGoap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MINE_ACTION, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = MAX_SUPPLY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Mine Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.state != POI_STATE.INACTIVE;
    }
    #endregion

    #region State Effects
    public void PreMineSuccess() {
        if (poiTarget is Ore) {
            Ore ore = poiTarget as Ore;
            _gainedSupply = ore.GetSupplyPerMine();
        }
        currentState.AddLogFiller(null, _gainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterMineSuccess() {
        if (poiTarget is Ore) {
            Ore ore = poiTarget as Ore;
            actor.AdjustSupply(_gainedSupply);
            ore.AdjustYield(-_gainedSupply);
        }
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget.gridTileLocation.structure.location, poiTarget.gridTileLocation.structure.ToString(), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
