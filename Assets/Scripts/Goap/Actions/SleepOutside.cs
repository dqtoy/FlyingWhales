using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SleepOutside : GoapAction {
    private Resting _restingTrait;

    public SleepOutside(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.SLEEP_OUTSIDE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEARBY;
        actionIconString = GoapActionStateDB.Sleep_Icon;
        //animationName = "Sleep Ground";
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void Perform() {
        base.Perform();
        //if (targetTile != null) {
            SetState("Rest Success");
        //} else {
        //    SetState("Rest Fail");
        //}
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetBaseCost() {
        return 65;
    }
    //public override void SetTargetStructure() {
    //    List<LocationStructure> choices = actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.WILDERNESS).Where(x => x.unoccupiedTiles.Count > 0).ToList();
    //    if (actor.specificLocation.HasStructure(STRUCTURE_TYPE.DWELLING)) {
    //        choices.AddRange(actor.specificLocation.GetStructuresOfType(STRUCTURE_TYPE.DWELLING).Where(x => x.unoccupiedTiles.Count > 0 && !x.IsOccupied()));
    //    }
    //    _targetStructure = choices[Utilities.rng.Next(0, choices.Count)];
    //    base.SetTargetStructure();
    //}
    public override void OnStopWhilePerforming() {
        if (currentState.name == "Rest Success") {
            RemoveTraitFrom(actor, "Resting");
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreRestSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        //actor.AdjustDoNotGetTired(1);
        _restingTrait = new Resting();
        actor.AddTrait(_restingTrait);
        currentState.SetAnimation("Sleep Ground");
        currentState.OverrideDuration(actor.currentSleepTicks);
    }
    private void PerTickRestSuccess() {
        actor.AdjustTiredness(70);
        actor.AdjustSleepTicks(-1);

        if (_restingTrait.lycanthropyTrait == null) {
            if (currentState.currentDuration == currentState.duration) {
                //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
                bool isTargettedByDrinkBlood = false;
                for (int i = 0; i < actor.targettedByAction.Count; i++) {
                    if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
                        isTargettedByDrinkBlood = true;
                        break;
                    }
                }
                if (isTargettedByDrinkBlood) {
                    currentState.OverrideDuration(currentState.duration + 1);
                }
            }
        } else {
            bool isTargettedByDrinkBlood = false;
            for (int i = 0; i < actor.targettedByAction.Count; i++) {
                if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
                    isTargettedByDrinkBlood = true;
                    break;
                }
            }
            if (currentState.currentDuration == currentState.duration) {
                //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
                if (isTargettedByDrinkBlood) {
                    currentState.OverrideDuration(currentState.duration + 1);
                } else {
                    if (!_restingTrait.hasTransformed) {
                        _restingTrait.CheckForLycanthropy(true);
                    }
                }
            } else {
                if (!isTargettedByDrinkBlood) {
                    _restingTrait.CheckForLycanthropy();
                }
            }
        }
    }
    private void AfterRestSuccess() {
        //actor.AdjustDoNotGetTired(-1);
        RemoveTraitFrom(actor, "Resting");
    }
    private void PreRestFail() {
        if (parentPlan != null && parentPlan.job != null && parentPlan.job.id == actor.sleepScheduleJobID) {
            actor.SetHasCancelledSleepSchedule(true);
        }
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