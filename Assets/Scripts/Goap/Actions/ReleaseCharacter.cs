using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReleaseCharacter : GoapAction {

    public ReleaseCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION, actor, poiTarget) {
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = HasAbductedOrRestrainedTrait;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = "Tool", targetPOI = actor }, HasItemTool);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Abducted", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Restrained", targetPOI = poiTarget });
    }
    public override bool PerformActualAction() {
        if (base.PerformActualAction()) {
            if (poiTarget is Character) {
                Character target = poiTarget as Character;
                if (actor.currentStructure == target.currentStructure) {
                    Trait abductedOrRestrained = target.GetTraitOr("Abducted", "Restrained");
                    target.RemoveTrait(abductedOrRestrained);
                    return true;
                }
            }
        }
        return false;
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region Preconditions
    private bool HasItemTool() {
        return actor.isHoldingItem && actor.tokenInInventory.name == "Tool";
    }
    #endregion

    #region Requirements
    protected bool HasAbductedOrRestrainedTrait() {
        if (poiTarget is Character) {
            Character target = poiTarget as Character;
            return target.GetTraitOr("Abducted", "Restrained") != null;
        }
        return false;
    }
    #endregion
}
