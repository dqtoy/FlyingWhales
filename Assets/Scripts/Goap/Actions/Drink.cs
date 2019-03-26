using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drink : GoapAction {
    public Drink(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DRINK, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        actionIconString = GoapActionStateDB.Eat_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            //SetState("Drink Success");
            if (poiTarget.GetTrait("Poisoned") != null) {
                SetState("Drink Poisoned");
            } else {
                SetState("Drink Success");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return Utilities.rng.Next(7, 10);
    }
    public override void FailAction() {
        base.FailAction();
        SetState("Target Missing");
    }
    #endregion

    #region State Effects
    public void PreDrinkSuccess() {
        actor.AdjustDoNotGetLonely(1);
    }
    public void PerTickDrinkSuccess() {
        actor.AdjustHappiness(8);
    }
    public void AfterDrinkSuccess() {
        actor.AdjustDoNotGetLonely(-1);
    }
    public void PreDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(1);
        poiTarget.RemoveTrait("Poisoned");
    }
    public void PerTickDrinkPoisoned() {
        actor.AdjustHappiness(8);
    }
    public void AfterDrinkPoisoned() {
        actor.AdjustDoNotGetLonely(-1);
        int chance = UnityEngine.Random.Range(0, 2);
        if (chance == 0) {
            actor.AddTrait("Sick");
        } else {
            actor.Death();
        }
    }
    public void PreTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return targetStructure.structureType == STRUCTURE_TYPE.INN;
    }
    #endregion
}
