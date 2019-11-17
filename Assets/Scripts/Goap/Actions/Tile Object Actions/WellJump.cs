using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class WellJump : GoapAction {

    public WellJump(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.WELL_JUMP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, targetPOI = actor });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        //if (!isTargetMissing) {
            SetState("Well Jump Success");
        //} else {
        //    SetState("Target Missing");
        //}
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 10;
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
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
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}