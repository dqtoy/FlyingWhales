using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RemovePoison : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public RemovePoison() : base(INTERACTION_TYPE.REMOVE_POISON) {
        actionIconString = GoapActionStateDB.Work_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_ITEM, conditionKey = SPECIAL_TOKEN.TOOL.ToString(), target = GOAP_EFFECT_TARGET.ACTOR }, HasItemTool);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Remove Poison Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 4;
    }
    #endregion

    #region State Effects
    public void PreRemovePoisonSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.structure.location, goapNode.poiTarget.gridTileLocation.structure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterRemovePoisonSuccess(ActualGoapNode goapNode) {
        //**Effect 1**: Remove Poisoned Trait from target table
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Poisoned");
        //**Effect 2**: Remove Tool from Actor's inventory
        if (goapNode.actor.HasTokenInInventory(SPECIAL_TOKEN.TOOL)) {
            goapNode.actor.ConsumeToken(goapNode.actor.GetToken(SPECIAL_TOKEN.TOOL));
        } else {
            //the actor does not have a tool, log for now
            Debug.LogWarning(goapNode.actor.name + " does not have a tool for removing poison! Poison was still removed, but thought you should know.");
        }
       
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            return poiTarget.traitContainer.GetNormalTrait("Poisoned") != null;
        }
        return false;
    }
    #endregion

    #region Preconditions
    private bool HasItemTool(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor.GetToken("Tool") != null;
    }
    #endregion
}

public class RemovePoisonTableData : GoapActionData {
    public RemovePoisonTableData() : base(INTERACTION_TYPE.REMOVE_POISON) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        return poiTarget.traitContainer.GetNormalTrait("Poisoned") != null;
    }
}