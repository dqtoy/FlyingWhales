using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformToWolfForm : GoapAction {

    public TransformToWolfForm(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    }
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
    #endregion

    #region State Effects
    public void AfterTransformSuccess() {
        Lycanthropy lycanthropy = actor.GetTrait("Lycanthropy") as Lycanthropy;
        lycanthropy.TurnToWolf();
    }
    #endregion
}
