using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChopWood : GoapAction {
    private const int MAX_SUPPLY = 50;
    private const int MIN_SUPPLY = 20;

    private int _gainedSupply;
    public ChopWood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHOP_WOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = MAX_SUPPLY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure && actor.gridTileLocation.IsAdjacentTo(poiTarget)) {
            SetState("Chop Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region State Effects
    public void PreChopSuccess() {
        if (poiTarget is Tree) {
            Tree tree = poiTarget as Tree;
            _gainedSupply = tree.GetSupplyPerMine();
        }
        currentState.AddLogFiller(null, _gainedSupply.ToString(), LOG_IDENTIFIER.STRING_1);
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterChopSuccess() {
        if (poiTarget is Tree) {
            Tree tree = poiTarget as Tree;
            actor.AdjustSupply(_gainedSupply);
            tree.AdjustYield(-_gainedSupply);
        }
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
