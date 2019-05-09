using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CureCharacter : GoapAction {

    public CureCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.CURE_CHARACTER, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    #region Overrides
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = "Healing Potion", targetPOI = actor }, HasItemInInventory);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Sick", targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Cure Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    //public void PreCureSuccess() {
    //    //**Pre Effect 1**: Prevent movement of Target
    //    (poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    //}
    public void AfterCureSuccess() {
        //**After Effect 1**: Reduce target's Sick trait
        if(parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);
        RemoveTraitFrom(poiTarget, "Sick");
        //**After Effect 2**: Remove Healing Potion from Actor's Inventory
        actor.ConsumeToken(actor.GetToken(SPECIAL_TOKEN.HEALING_POTION));
        //**After Effect 3**: Allow movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(-1);
    }
    #endregion

    #region Preconditions
    private bool HasItemInInventory() {
        return actor.HasItem("Healing Potion");
        //return true;
    }
    #endregion
}
