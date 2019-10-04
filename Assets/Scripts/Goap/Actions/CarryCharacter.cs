using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryCharacter : GoapAction {
    public CarryCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget }, HasAbductedOrRestrainedTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY_WITH_CONSENT, conditionKey = actor, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        //rather than checking location check if the character is not in anyone elses party and is still active
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) { 
            SetState("Carry Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Preconditions
    private bool HasAbductedOrRestrainedTrait() {
        Character target = poiTarget as Character;
        return target.GetNormalTrait("Restrained") != null;
    }
    #endregion

    #region State Effects
    //public void PreCarrySuccess() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterCarrySuccess() {
        //if (parentPlan != null && parentPlan.job != null) {
        //    parentPlan.job.SetCannotOverrideJob(true);//Carry should not be overrideable if the character is actually already carrying another character.
        //}
        Character target = poiTarget as Character;
        actor.ownParty.AddCharacter(target);
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}

public class CarryCharacterData : GoapActionData {
    public CarryCharacterData() : base(INTERACTION_TYPE.CARRY_CHARACTER) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}
