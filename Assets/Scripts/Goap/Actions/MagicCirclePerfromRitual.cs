using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCirclePerformRitual : GoapAction {
    public MagicCirclePerformRitual(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Perform Ritual";
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
            TIME_IN_WORDS.AFTER_MIDNIGHT,
        };
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 20, targetPOI = actor }, () => HasSupply(20));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.ADD_TRAIT, conditionKey = "Ritualized", targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation != null && poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure 
            && actor.gridTileLocation.IsAdjacentTo(poiTarget)) {
            if (poiTarget.state != POI_STATE.INACTIVE) {
                SetState("Perform Ritual Success");
            } else {
                SetState("Perform Ritual Fail");
            }
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
        SetState("Perform Ritual Fail");
    }
    #endregion

    #region State Effects
    public void PrePerformRitualSuccess() {
        //**Pre Effect 1**: Change Magic Circle's status to Inactive
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
    }
    public void AfterPerformRitualSuccess() {
        //**After Effect 1**: Change Magic Circle's status to Active
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
        //**After Effect 2**: Actor gains Ritualized trait, Actor loses 20 Supply
        actor.AddTrait("Ritualized");
        actor.AdjustSupply(-20);
    }
    public void PreTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
