using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevertToNormalForm : GoapAction {

    public RevertToNormalForm(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REVERT_TO_NORMAL, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        SetState("Transform Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 5;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Stroll Fail");
    //}
    protected override void CreateThoughtBubbleLog() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        thoughtBubbleLog = new Log(GameManager.Instance.Today(), "GoapAction", this.GetType().ToString(), "thought_bubble");
        if (thoughtBubbleLog != null) {
            thoughtBubbleLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            thoughtBubbleLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(lycanthropy.data.race), LOG_IDENTIFIER.STRING_1);
        }
    }
    #endregion

    #region State Effects
    public void PreTransformSuccess() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        currentState.AddLogFiller(null, Utilities.GetNormalizedSingularRace(lycanthropy.data.race), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterTransformSuccess() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        lycanthropy.RevertToNormal();
    }
    public void PreTargetMissing() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        currentState.AddLogFiller(null, Utilities.GetNormalizedSingularRace(lycanthropy.data.race), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
}
