using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicCirclePerformRitual : GoapAction {
    protected override string failActionState { get { return "Perform Ritual Fail"; } }

    public MagicCirclePerformRitual(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Perform Ritual";
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //    TIME_IN_WORDS.AFTER_MIDNIGHT,
        //};
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = 0, targetPOI = actor }, () => HasSupply(20));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Perform Ritual Success");
        } else {
            if (!poiTarget.IsAvailable()) {
                SetState("Perform Ritual Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Perform Ritual Success") {
            poiTarget.SetPOIState(POI_STATE.ACTIVE);
        }
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Perform Ritual Fail");
    //}
    #endregion

    #region Requirement
    private bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void PrePerformRitualSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //**Pre Effect 1**: Change Magic Circle's status to Inactive
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
    }
    public void AfterPerformRitualSuccess() {
        //**After Effect 1**: Change Magic Circle's status to Active
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
        //**After Effect 2**: Actor gains Ritualized trait, Actor loses 20 Supply
        AddTraitTo(actor, "Ritualized", actor);
        actor.AdjustSupply(-20);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
