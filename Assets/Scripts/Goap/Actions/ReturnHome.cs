using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;

    protected override string failActionState { get { return "Return Home Failed"; } }

    public ReturnHome(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RETURN_HOME, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        showIntelNotification = false;
        shouldAddLogs = false;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetTile != null) {
            SetState("Return Home Success");
        } else {
            SetState("Return Home Failed");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Return Home Failed");
    //}
    public override void SetTargetStructure() {
        _targetStructure = actor.homeStructure;
        base.SetTargetStructure();
    }
    #endregion

    #region State Effects
    public void PreReturnHomeSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreReturnHomeFailed() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}
