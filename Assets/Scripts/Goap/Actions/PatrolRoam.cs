using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolRoam : GoapAction {
    protected override string failActionState { get { return "Patrol Fail"; } }

    public PatrolRoam(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PATROL_ROAM, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //showIntelNotification = false;
        //shouldAddLogs = false;
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        if (targetTile != null) {
            SetState("Patrol Success");
        } else {
            SetState("Patrol Fail");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 5;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Stroll Fail");
    //}
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
    }
    #endregion

    #region State Effects
    public void PrePatrolSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PrePatrolFail() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}
