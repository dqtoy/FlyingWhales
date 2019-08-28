using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WellJump : GoapAction {

    public WellJump(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.WELL_JUMP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //if (!isTargetMissing) {
            SetState("Well Jump Success");
        //} else {
        //    SetState("Target Missing");
        //}
    }
    protected override int GetCost() {
        return 10;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void AfterWellJumpSuccess() {
        actor.Death("suicide", this, _deathLog: currentState.descriptionLog);
    }
    #endregion
}

public class WellJumpData : GoapActionData {
    public WellJumpData() : base(INTERACTION_TYPE.WELL_JUMP) {
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}