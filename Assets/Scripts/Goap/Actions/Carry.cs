using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carry : GoapAction {

    //protected override bool isTargetMissing {
    //    get {
    //        bool targetMissing = false;
    //        if (parentPlan != null && parentPlan.job != null && parentPlan.job.allowDeadTargets) {
    //            targetMissing = poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
    //                || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) || !(poiTarget as Character).isDead;
    //        } else {
    //            targetMissing = !poiTarget.IsAvailable() || poiTarget.gridTileLocation == null || actor.specificLocation != poiTarget.specificLocation
    //                || !(actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation));
    //        }

    //        if (targetMissing) {
    //            return targetMissing;
    //        } else {
    //            if (actor != poiTarget) {
    //                Invisible invisible = poiTarget.GetNormalTrait("Invisible") as Invisible;
    //                if (invisible != null && !invisible.charactersThatCanSee.Contains(actor)) {
    //                    return true;
    //                }
    //            }
    //            return targetMissing;
    //        }
    //    }
    //}

    public Carry(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget });
    }
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
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
        return actor != poiTarget;
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
    //public void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class CarryData : GoapActionData {
    public CarryData() : base(INTERACTION_TYPE.CARRY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget && poiTarget is Character && (poiTarget as Character).HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER);
    }
}
