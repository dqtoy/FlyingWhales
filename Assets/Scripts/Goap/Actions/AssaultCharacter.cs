using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssaultCharacter : GoapAction {
    public AssaultCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASSAULT_ACTION_NPC, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_NON_POSITIVE_TRAIT, conditionKey = "Disabler", targetPOI = poiTarget });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            Character target = poiTarget as Character;
            target.AddTrait("Unconscious");
            target.AddTrait("Injured");
            return true;
        }
        return false;
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
}
