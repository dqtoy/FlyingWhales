using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoTo : GoapAction {

    public GoTo(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.GO_TO, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_VISION, targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        SetState("Goto Success");
    }
    protected override int GetBaseCost() {
        return 15;
    }
    #endregion

    //#region State Effects
    //public void AfterSpookedSuccess() {
    //    //if (parentPlan != null && parentPlan.job != null) {
    //    //    parentPlan.job.SetCannotOverrideJob(true);//Carry should not be overrideable if the character is actually already carrying another character.
    //    //}
    //    Character target = poiTarget as Character;
    //    actor.ownParty.AddCharacter(target);
    //}
    //#endregion
}

public class GoToData : GoapActionData {
    public GoToData() : base(INTERACTION_TYPE.GO_TO) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}