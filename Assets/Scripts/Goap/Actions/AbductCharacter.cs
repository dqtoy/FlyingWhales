using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbductCharacter : GoapAction {
    public AbductCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ABDUCT_ACTION, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _isStealthAction = true;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget }, HasNonPositiveDisablerTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetCharacterMissing) {
            if (!HasOtherCharacterInRadius()) {
                SetState("Abduct Success");
            } else {
                parentPlan.SetDoNotRecalculate(true);
                SetState("Abduct Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    public override void DoAction(GoapPlan plan) {
        SetTargetStructure();
        base.DoAction(plan);
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

    //#region Preconditions
    //private bool HasNonPositiveDisablerTrait() {
    //    Character target = poiTarget as Character;
    //    return target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_EFFECT.NEUTRAL, TRAIT_TYPE.DISABLER);
    //}
    //#endregion

    #region State Effects
    //public void PreAbductSuccess() {
        //currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterAbductSuccess() {
        Character target = poiTarget as Character;
        Restrained restrainedTrait = new Restrained();
        target.AddTrait(restrainedTrait, actor);

        AddActualEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion

    
}