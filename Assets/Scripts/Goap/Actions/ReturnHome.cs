﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ReturnHome : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReturnHome() : base(INTERACTION_TYPE.RETURN_HOME) {
        showIntelNotification = false;
        shouldAddLogs = false;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        actionIconString = GoapActionStateDB.No_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Return Home Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    public override LocationStructure GetTargetStructure(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (actor.homeStructure != null) {
            return actor.homeStructure;
        } else {
            return actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        }
    }
    #endregion

    #region State Effects
    public void PreReturnHomeSuccess(ActualGoapNode goapNode) {
        GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
        goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    }
    #endregion
}

public class ReturnHomeData : GoapActionData {
    public ReturnHomeData() : base(INTERACTION_TYPE.RETURN_HOME) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
    }
}
