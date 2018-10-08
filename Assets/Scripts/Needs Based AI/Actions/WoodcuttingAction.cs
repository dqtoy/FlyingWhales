using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodcuttingAction : CharacterAction {

    public WoodcuttingAction() : base(ACTION_TYPE.WOODCUTTING) {

    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }

        int numOfResources = Random.Range(this.actionData.minResourceGiven, this.actionData.maxResourceGiven);
        party.icharacterObject.AdjustResource(this.actionData.resourceGiven, numOfResources);
        ActionSuccess(targetObject);
    }
    public override CharacterAction Clone() {
        WoodcuttingAction action = new WoodcuttingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override SCHEDULE_ACTION_CATEGORY GetSchedActionCategory() {
        return SCHEDULE_ACTION_CATEGORY.WORK;
    }
    #endregion
}
