using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacter : GoapAction {

    public ReleaseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = () => Requirement();
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = "Tool", targetPOI = TARGET_POI.ACTOR }, HasItemTool);
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted" });
        AddEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained" });
    }
    #endregion

    private bool Requirement() {
        return true;
    }

    #region Preconditions
    private bool HasItemTool() {
        return actor.isHoldingItem && actor.tokenInInventory.name == "Tool";
    }
    #endregion
}
