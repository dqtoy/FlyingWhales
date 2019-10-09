using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckOut : GoapAction {

    public CheckOut(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CHECK_OUT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.TARGET_IN_VISION;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Checkout Success");
    }
    protected override int GetCost() {
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

public class CheckOutData : GoapActionData {
    public CheckOutData() : base(INTERACTION_TYPE.CHECK_OUT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}