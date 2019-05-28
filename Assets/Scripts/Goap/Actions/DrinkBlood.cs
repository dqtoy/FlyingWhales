using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrinkBlood : GoapAction {
    protected override string failActionState { get { return "Drink Fail"; } }

    public DrinkBlood(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DRINK_BLOOD, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Eat_Icon;
        doesNotStopTargetCharacter = true;
    }

    #region Overrides
    protected override void ConstructRequirementOnBuildGoapTree() {
        _requirementOnBuildGoapTreeAction = RequirementOnBuildGoapTree;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.isStarving) {
            AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", targetPOI = poiTarget }, HasUnconsciousOrRestingTarget);
        }
        if (actor.GetTrait("Vampiric") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        }
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Lethargic", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing) {
            Character target = poiTarget as Character;
            if(target.GetTraitOr("Unconscious", "Resting") != null) {
                SetState("Drink Success");
            } else {
                SetState("Drink Fail");
            }
        } else {
            if (!poiTarget.IsAvailable()) {
                SetState("Drink Fail");
            } else {
                SetState("Target Missing");
            }
        }
    }
    protected override int GetCost() {
        return 1;
    }
    public override void OnStopActionDuringCurrentState() {
        if (currentState.name == "Drink Success") {
            actor.AdjustDoNotGetHungry(-1);
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return true;
    }
    protected bool RequirementOnBuildGoapTree() {
        if (!actor.isStarving) {
            Character target = poiTarget as Character;
            return target.GetTraitOr("Unconscious", "Resting") != null;
        }
        return true;
    }
    #endregion

    #region Preconditions
    private bool HasUnconsciousOrRestingTarget() {
        Character target = poiTarget as Character;
        return target.GetTraitOr("Unconscious", "Resting") != null;
    }
    #endregion

    #region Effects
    private void PreDrinkSuccess() {
        SetCommittedCrime(CRIME.ABERRATION, new Character[] { actor });
        //poiTarget.SetPOIState(POI_STATE.INACTIVE);
        actor.AdjustDoNotGetHungry(1);
    }
    private void PerTickDrinkSuccess() {
        actor.AdjustFullness(12);
    }
    private void AfterDrinkSuccess() {
        //poiTarget.SetPOIState(POI_STATE.ACTIVE);
        actor.AdjustDoNotGetHungry(-1);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < 95) {
            Lethargic lethargic = new Lethargic();
            AddTraitTo(poiTarget, lethargic, actor);
        } else {
            Vampiric vampiric = new Vampiric();
            AddTraitTo(poiTarget, vampiric, actor);
        }
    }
    //private void PreDrinkFail() {
    //    currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void PreTargetMissing() {
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //private void AfterTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

}
