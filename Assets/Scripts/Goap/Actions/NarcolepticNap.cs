using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarcolepticNap : GoapAction {

    public Resting restingTrait { get; private set; }

    public NarcolepticNap(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.NARCOLEPTIC_NAP, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Nap Success");
    }
    public override void DoAction() {
        SetTargetStructure();
        base.DoAction();
    }
    protected override int GetCost() {
        return 1;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Nap Success") {
            RemoveTraitFrom(actor, "Resting");
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor == poiTarget;
    }
    #endregion

    #region State Effects
    private void PreNapSuccess() {
        restingTrait = new Resting();
        actor.AddTrait(restingTrait);
    }
    private void PerTickNapSuccess() {
        actor.AdjustTiredness(30);

        if (restingTrait.lycanthropyTrait == null) {
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
                    if (!restingTrait.hasTransformed) {
                        restingTrait.CheckForLycanthropy(true);
                    }
                }
            } else {
                if (!isTargettedByDrinkBlood) {
                    restingTrait.CheckForLycanthropy();
                }
            }
        }
    }
    private void AfterNapSuccess() {
        RemoveTraitFrom(actor, "Resting");
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
