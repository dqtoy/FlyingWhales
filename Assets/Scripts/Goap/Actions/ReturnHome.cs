using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnHome : GoapAction {
    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;

    protected override string failActionState { get { return "Return Home Failed"; } }

    public ReturnHome(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.RETURN_HOME, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        SetShowIntelNotification(false);
        shouldAddLogs = false;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
    }

    #region Overrides
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.NONE, targetPOI = actor });
    //}
    public override void PerformActualAction() {
        //if (targetTile != null) {
        //} else {
        //    SetState("Return Home Failed");
        //}
        base.PerformActualAction();
        SetState("Return Home Success");
    }
    public override LocationGridTile GetTargetLocationTile() {
        if(targetStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
            return null;
        }
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    protected override int GetCost() {
        return 3;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Return Home Failed");
    //}
    public override void SetTargetStructure() {
        if (actor.homeStructure != null) {
            _targetStructure = actor.homeStructure;
        } else {
            _targetStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
        base.SetTargetStructure();
    }
    #endregion

    #region State Effects
    public void PreReturnHomeSuccess() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    public void PreReturnHomeFailed() {
        currentState.AddLogFiller(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}

public class ReturnHomeData : GoapActionData {
    public ReturnHomeData() : base(INTERACTION_TYPE.RETURN_HOME) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }
}
