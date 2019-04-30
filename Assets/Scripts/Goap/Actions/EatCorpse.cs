using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatCorpse : GoapAction {
    protected override string failActionState { get { return "Eat Fail"; } }

    public EatCorpse(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_CORPSE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            SetState("Eat Success");
        } else {
            if (poiTarget.state == POI_STATE.INACTIVE) {
                SetState("Eat Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Eat Fail");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Eat Success") {
            actor.AdjustDoNotGetHungry(-1);
            poiTarget.SetPOIState(POI_STATE.ACTIVE);
        }
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(8);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
        Corpse corpse = poiTarget as Corpse;
        poiTarget.gridTileLocation.structure.RemoveCorpse(corpse.character);
    }
    private void PreEatFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    private void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.state != POI_STATE.INACTIVE;
    }
    #endregion
}
