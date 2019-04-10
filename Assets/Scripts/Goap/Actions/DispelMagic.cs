using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispelMagic : GoapAction {

    public DispelMagic(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DISPEL_MAGIC, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Social_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor }, () => HasTrait(actor, "Ritualized"));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Cursed", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        SetState("Dispel Magic Success");
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreDispelMagicSuccess() {
        //**Pre Effect 1**: Prevent movement of Target
        (poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    }
    public void AfterDispelMagicSuccess() {
        //**After Effect 1**: Reduce all of target's Enchantment type traits
        RemoveTraitsOfType(poiTarget, TRAIT_TYPE.ENCHANTMENT);
        //**After Effect 2**: Actor loses Ritualized trait.
        RemoveTraitFrom(actor, "Ritualized");
        //**After Effect 3**: Allow movement of Target
        (poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);

    }
    #endregion
}
