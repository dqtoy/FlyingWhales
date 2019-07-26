using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drink : GoapAction {
    public Trait poisonedTrait { get; private set; }

    public Drink(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DRINK, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //shouldIntelNotificationOnlyIfActorIsActive = true;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
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
            poisonedTrait = poiTarget.GetNormalTrait("Poisoned");
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
        return Utilities.rng.Next(15, 26);
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
        actor.AdjustHappiness(14);
    }
    public void AfterDrinkSuccess() {
        actor.AdjustDoNotGetLonely(-1);
        AddTraitTo(actor, "Drunk", actor);
    }
    public void PreDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(1);
        RemoveTraitFrom(poiTarget, "Poisoned");
    }
    public void PerTickDrinkPoisoned() {
        actor.AdjustHappiness(14);
    }
    public void AfterDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(-1);
        int chance = UnityEngine.Random.Range(0, 2);
        if (chance == 0) {
            Sick sick = new Sick();
            for (int i = 0; i < poisonedTrait.responsibleCharacters.Count; i++) {
                AddTraitTo(actor, sick, poisonedTrait.responsibleCharacters[i]);
            }
        } else {
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
        return targetStructure.structureType == STRUCTURE_TYPE.INN && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion
}
                                                                                  