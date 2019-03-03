using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryCharacter : GoapAction {
    public CarryCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CARRY_CHARACTER, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget }, HasAbductedOrRestrainedTrait);
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = actor, targetPOI = poiTarget });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            Character target = poiTarget as Character;
            actor.ownParty.AddCharacter(target);
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

    #region Preconditions
    private bool HasAbductedOrRestrainedTrait() {
        Character target = poiTarget as Character;
        return target.GetTrait("Abducted") != null;
    }
    #endregion
}
