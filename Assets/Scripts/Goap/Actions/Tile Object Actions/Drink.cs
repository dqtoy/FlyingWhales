using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drink : GoapAction {
    public Poisoned poisonedTrait { get; private set; }
    private string poisonedResult;

    public Drink(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DRINK, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //shouldIntelNotificationOnlyIfActorIsActive = true;
        if (actor.GetNormalTrait("Alcoholic") != null) {
            validTimeOfDays = new TIME_IN_WORDS[] {
                TIME_IN_WORDS.MORNING,
                TIME_IN_WORDS.AFTERNOON,
                TIME_IN_WORDS.AFTER_MIDNIGHT,
                TIME_IN_WORDS.EARLY_NIGHT,
                TIME_IN_WORDS.LATE_NIGHT,
            };
        } else {
            validTimeOfDays = new TIME_IN_WORDS[] {
                TIME_IN_WORDS.EARLY_NIGHT,
                TIME_IN_WORDS.LATE_NIGHT,
            };
        }
        
        actionIconString = GoapActionStateDB.Drink_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            //SetState("Drink Success");
            poisonedTrait = poiTarget.GetNormalTrait("Poisoned") as Poisoned;
            if (poisonedTrait != null) {
                SetState("Drink Poisoned");
            } else {
                SetState("Drink Success");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        //**Cost**: 15 - 26 (If Actor is alcoholic 5 - 19)
        if (actor.GetNormalTrait("Alcoholic") != null) {
            return Utilities.rng.Next(5, 20);
        }
        return Utilities.rng.Next(15, 27);
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Drink Success" || currentState.name == "Drink Poisoned") {
            actor.AdjustDoNotGetLonely(-1);
        }
    }
    #endregion

    #region State Effects
    public void PreDrinkSuccess() {
        actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickDrinkSuccess() {
        actor.AdjustHappiness(500);
    }
    public void AfterDrinkSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        AddTraitTo(actor, "Drunk", actor);
    }
    public void PreDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(1);
        RemoveTraitFrom(poiTarget, "Poisoned");
        Log log = null;
        WeightedDictionary<string> result = poisonedTrait.GetResultWeights();
        string res = result.PickRandomElementGivenWeights();
        if (res == "Sick") {
            string logKey = "drink poisoned_sick";
            poisonedResult = "Sick";
            if (actor.GetNormalTrait("Robust") != null) {
                poisonedResult = "Robust";
                logKey = "drink poisoned_robust";
            }
            log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", logKey, this);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        } else if (res == "Death") {
            log = new Log(GameManager.Instance.Today(), "GoapAction", "Drink", "drink poisoned_killed", this);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            poisonedResult = "Death";
        }
        currentState.OverrideDescriptionLog(log);
    }
    public void PerTickDrinkPoisoned() {
        actor.AdjustHappiness(200);
    }
    public void AfterDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(-1);
        if (poisonedResult == "Sick") {
            for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
                AddTraitTo(actor, poisonedResult, poisonedTrait.responsibleCharacters[i]);
            }
        } else if (poisonedResult == "Death") {
            if (parentPlan.job != null) {
                parentPlan.job.SetCannotCancelJob(true);
            }
            SetCannotCancelAction(true);
            actor.Death("poisoned", deathFromAction: this);
        }
    }
    public void PreTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.GetNormalTrait("Agoraphobia") == null;
    }
    #endregion
}

public class DrinkData : GoapActionData {
    public DrinkData() : base(INTERACTION_TYPE.DRINK) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.gridTileLocation != null &&  poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && actor.GetNormalTrait("Agoraphobia") == null;
    }
}