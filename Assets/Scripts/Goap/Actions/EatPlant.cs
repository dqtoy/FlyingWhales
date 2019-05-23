using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatPlant : GoapAction {
    protected override string failActionState { get { return "Eat Fail"; } }

    public EatPlant(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.EAT_PLANT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Eat_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
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
        if (actor.GetTrait("Herbivore") != null) {
            return 25;
        } else {
            return 50;
        }
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Eat Fail");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Eat Success") {
            actor.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Effects
    private void PreEatSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
        //actor.AddTrait("Eating");
    }
    private void PerTickEatSuccess() {
        actor.AdjustFullness(10);
    }
    private void AfterEatSuccess() {
        actor.AdjustDoNotGetHungry(-1);
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
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
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.state != POI_STATE.INACTIVE;
    }
    #endregion
}
