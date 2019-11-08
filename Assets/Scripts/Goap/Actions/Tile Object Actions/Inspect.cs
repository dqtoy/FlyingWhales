using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspect : GoapAction {

    private int _gainedSupply;
    public Inspect(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.INSPECT, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    if (actor.GetNormalTrait("Curious") != null) {
    //        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //    }
    //}
    public override void Perform() {
        base.Perform();
        if (!isTargetMissing) {
            SetState("Inspect Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetBaseCost() {
        return 4;
    }
    public override void AfterAfterEffect() {
        base.AfterAfterEffect();
        if(currentState.name == "Inspect Success") {
            //Log result;
            (poiTarget as TileObject).OnInspect(actor); //, out result
            //if (result != null) {
            //    currentState.AddLogFiller(null, Utilities.LogReplacer(result), LOG_IDENTIFIER.STRING_1);
            //} else {
            //    currentState.AddLogFiller(null, "and nothing happened", LOG_IDENTIFIER.STRING_1);
            //}
        }
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region State Effects
    public void PreInspectSuccess() {
        //currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    public void AfterInspectSuccess() {
        if (poiTarget is TileObject) {
            TileObject to = poiTarget as TileObject;
            //Curious curios = actor.GetNormalTrait("Curious") as Curious;
            actor.defaultCharacterTrait.AddAlreadyInspectedObject(to);
        } 
    }
    public void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterTargetMissing() {
        actor.RemoveAwareness(poiTarget);
    }
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