using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryCharacter : GoapAction {
    public CarryCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget }, HasAbductedOrRestrainedTrait);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        if (poiTarget.gridTileLocation.structure == actor.gridTileLocation.structure) {
            SetState("Carry Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region Preconditions
    private bool HasAbductedOrRestrainedTrait() {
        Character target = poiTarget as Character;
        return target.GetTrait("Abducted") != null;
    }
    #endregion

    #region State Effects
    public void PreCarrySuccess() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterCarrySuccess() {
        Character target = poiTarget as Character;
        actor.ownParty.AddCharacter(target);
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget as Character, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
    #endregion
}
