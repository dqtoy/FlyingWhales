using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class FeelingSpooked : GoapAction {

    public FeelingSpooked(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.FEELING_SPOOKED, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Spooked Success");
    }
    protected override int GetCost() {
        return 10;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
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

public class FeelingSpookedData : GoapActionData {
    public FeelingSpookedData() : base(INTERACTION_TYPE.FEELING_SPOOKED) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}