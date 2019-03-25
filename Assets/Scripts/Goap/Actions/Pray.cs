using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pray : GoapAction {
    public Pray(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.PRAY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        this.goapName = "Pray";
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Joy_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (targetTile.occupant != null && targetTile.occupant != actor) {
            SetState("Pray Failed");
        } else {
            SetState("Pray Success");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return Utilities.rng.Next(7, 14);
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Pray Failed");
    }
    #endregion

    #region State Effects
    public void PrePraySuccess() {
        actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickPraySuccess() {
        actor.AdjustHappiness(8);
    }
    public void AfterPraySuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion
}
