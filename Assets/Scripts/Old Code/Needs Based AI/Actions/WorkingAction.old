using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkingAction : CharacterAction {

    public WorkingAction() : base(ACTION_TYPE.WORKING) {

    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override CharacterAction Clone() {
        WorkingAction action = new WorkingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override SCHEDULE_ACTION_CATEGORY GetSchedActionCategory() {
        return SCHEDULE_ACTION_CATEGORY.WORK;
    }
    #endregion
}
