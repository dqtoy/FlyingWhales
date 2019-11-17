using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Inspect : GoapAction {

    public Inspect() : base(INTERACTION_TYPE.INSPECT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Inspect Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 4;
    }
    //TODO:
    //public override void AfterAfterEffect() {
    //    base.AfterAfterEffect();
    //    if(currentState.name == "Inspect Success") {
    //        //Log result;
    //        (poiTarget as TileObject).OnInspect(actor); //, out result
    //        //if (result != null) {
    //        //    currentState.AddLogFiller(null, Utilities.LogReplacer(result), LOG_IDENTIFIER.STRING_1);
    //        //} else {
    //        //    currentState.AddLogFiller(null, "and nothing happened", LOG_IDENTIFIER.STRING_1);
    //        //}
    //    }
    //}
    #endregion

    #region Requirements
   protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreInspectSuccess(ActualGoapNode goapNode) {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        goapNode.action.states[goapNode.currentStateName].AddLogFiller(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterInspectSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is TileObject) {
            TileObject to = goapNode.poiTarget as TileObject;
            goapNode.actor.defaultCharacterTrait.AddAlreadyInspectedObject(to);
        } 
    }
    //public void PreTargetMissing(ActualGoapNode goapNode) {
    //    currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //    currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void AfterTargetMissing(ActualGoapNode goapNode) {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion
}

public class InspectData : GoapActionData {
    public InspectData() : base(INTERACTION_TYPE.INSPECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}