using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using System.Linq;

public class SleepOutside : GoapAction {

    public SleepOutside() : base(INTERACTION_TYPE.SLEEP_OUTSIDE) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //animationName = "Sleep Ground";
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Rest Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 65;
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
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        currentState.SetAnimation("Sleep Ground");
        //TODO: currentState.OverrideDuration(goapNode.actor.currentSleepTicks);
    }
    private void PerTickRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustTiredness(70);
        goapNode.actor.AdjustSleepTicks(-1);

        //TODO:
        //if (_restingTrait.lycanthropyTrait == null) {
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
        //            if (!_restingTrait.hasTransformed) {
        //                _restingTrait.CheckForLycanthropy(true);
        //            }
        //        }
        //    } else {
        //        if (!isTargettedByDrinkBlood) {
        //            _restingTrait.CheckForLycanthropy();
        //        }
        //    }
        //}
    }
    private void AfterRestSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    #endregion
}

public class SleepOutsideData : GoapActionData {
    public SleepOutsideData() : base(INTERACTION_TYPE.SLEEP_OUTSIDE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
}