using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feed : GoapAction {
    private Character _target;

    public Feed(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FEED, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        _target = poiTarget as Character;
        actionIconString = GoapActionStateDB.Eat_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    //protected override void ConstructRequirement() {
    //    _requirementAction = Requirement;
    //}
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = actor }, () => HasSupply(10));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation != null && (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsAdjacentTo(poiTarget))) {
            SetState("Feed Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    #endregion

    #region Effects
    private void PreFeedSuccess() {
        //currentState.AddLogFiller(_target, _target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        _target.AdjustDoNotGetHungry(1);
        actor.AdjustSupply(-10);
    }
    private void PerTickFeedSuccess() {
        _target.AdjustFullness(12);
    }
    private void AfterFeedSuccess() {
        _target.AdjustDoNotGetHungry(-1);
    }
    //private void PreTargetMissing() {
        //currentState.AddLogFiller(_target, _target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    #endregion

    //#region Requirements
    //protected bool Requirement() {
    //    return poiTarget.state != POI_STATE.INACTIVE;
    //}
    //#endregion
}
