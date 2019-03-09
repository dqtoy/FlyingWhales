using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductCharacter : GoapAction {
    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_ACTION, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Abduct Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    public override bool IsHalted() {
        TIME_IN_WORDS timeInWords = GameManager.GetCurrentTimeInWordsOfTick();
        if(timeInWords == TIME_IN_WORDS.EARLY_NIGHT || timeInWords == TIME_IN_WORDS.LATE_NIGHT || timeInWords == TIME_IN_WORDS.AFTER_MIDNIGHT) {
            return false;
        }
        if(poiTarget.gridTileLocation.structure.structureType == STRUCTURE_TYPE.WILDERNESS || poiTarget.gridTileLocation.structure.charactersHere.Count == 1) {
            return false;
        }
        return true;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(actor != poiTarget) {
            Character target = poiTarget as Character;
            return target.GetTrait("Abducted") == null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasNonPositiveDisablerTrait() {
        Character target = poiTarget as Character;
        return target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER);
    }
    #endregion

    #region State Effects
    public void PreAbductSuccess() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterAbductSuccess() {
        Character target = poiTarget as Character;
        Abducted abductedTrait = new Abducted(target.homeArea);
        target.AddTrait(abductedTrait, actor);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}