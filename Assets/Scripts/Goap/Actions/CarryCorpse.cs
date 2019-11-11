using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryCorpse : GoapAction {

    protected override bool isTargetMissing {
        get {
            return poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
              || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || !(poiTarget as Character).isDead;
        }
    }

    public CarryCorpse(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY_CORPSE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        //AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = poiTarget, targetPOI = poiTarget }, IsTargetDead);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        //rather than checking location check if the character is not in anyone elses party and is still active
        Character target = poiTarget as Character;
        if (!isTargetMissing && target.IsInOwnParty()) {
            SetState("Carry Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
        return 1;
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        Character targetCharacter = poiTarget as Character;
        if (actor == targetCharacter) {
            return false;
        }
        if (!targetCharacter.isDead) {
            return false;
        }
        return true;
    }
    #endregion

    #region State Effects
    public void AfterCarrySuccess() {
        //if (parentPlan != null && parentPlan.job != null) {
        //    parentPlan.job.SetCannotOverrideJob(true);//Carry should not be overrideable if the character is actually already carrying another character.
        //}
        Character target = poiTarget as Character;
        actor.ownParty.AddCharacter(target);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}

public class CarryCorpseData : GoapActionData {
    public CarryCorpseData() : base(INTERACTION_TYPE.CARRY_CORPSE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character targetCharacter = poiTarget as Character;
        if (actor == targetCharacter) {
            return false;
        }
        if (!targetCharacter.isDead) {
            return false;
        }
        return true;
    }
}