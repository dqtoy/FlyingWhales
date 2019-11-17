using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
public class NarcolepticNap : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public NarcolepticNap() : base(INTERACTION_TYPE.NARCOLEPTIC_NAP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Nap Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest target, object[] otherData) {
        base.OnStopWhilePerforming(actor, target, otherData);
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreNapSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.SetAnimation("Sleep Ground");
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
    }
    private void PerTickNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustTiredness(30);

        //TODO:
        //if (restingTrait.lycanthropyTrait == null) {
        //    if (currentState.currentDuration == currentState.duration) {
        //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //        bool isTargettedByDrinkBlood = false;
        //        for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //            if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //                isTargettedByDrinkBlood = true;
        //                break;
        //            }
        //        }
        //        if (isTargettedByDrinkBlood) {
        //            currentState.OverrideDuration(currentState.duration + 1);
        //        }
        //    }
        //} else {
        //    bool isTargettedByDrinkBlood = false;
        //    for (int i = 0; i < actor.targettedByAction.Count; i++) {
        //        if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
        //            isTargettedByDrinkBlood = true;
        //            break;
        //        }
        //    }
        //    if (currentState.currentDuration == currentState.duration) {
        //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
        //        if (isTargettedByDrinkBlood) {
        //            currentState.OverrideDuration(currentState.duration + 1);
        //        } else {
        //            if (!restingTrait.hasTransformed) {
        //                restingTrait.CheckForLycanthropy(true);
        //            }
        //        }
        //    } else {
        //        if (!isTargettedByDrinkBlood) {
        //            restingTrait.CheckForLycanthropy();
        //        }
        //    }
        //}
    }
    private void AfterNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    #endregion
}

public class NarcolepticNapData : GoapActionData {
    public NarcolepticNapData() : base(INTERACTION_TYPE.NARCOLEPTIC_NAP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}
