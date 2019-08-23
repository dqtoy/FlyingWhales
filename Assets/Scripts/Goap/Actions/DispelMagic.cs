using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispelMagic : GoapAction {

    public DispelMagic(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.DISPEL_MAGIC, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Ritualized", targetPOI = actor }, () => HasTrait(actor, "Ritualized"));
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Cursed", targetPOI = poiTarget });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Reanimated", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Dispel Magic Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    public void PreDispelMagicSuccess() {
        //**Pre Effect 1**: Prevent movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    }
    public void AfterDispelMagicSuccess() {
        //**After Effect 1**: Reduce all of target's Enchantment type traits
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        RemoveTraitsOfType(poiTarget, TRAIT_TYPE.ENCHANTMENT);
        //**After Effect 2**: Actor loses Ritualized trait.
        RemoveTraitFrom(actor, "Ritualized");
        //**After Effect 3**: Allow movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);

    }
    #endregion
}

public class DispelMagicData : GoapActionData {
    public DispelMagicData() : base(INTERACTION_TYPE.DISPEL_MAGIC) {
    }
}
