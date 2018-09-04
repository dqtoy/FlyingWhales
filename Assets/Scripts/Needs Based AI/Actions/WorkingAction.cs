using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkingAction : CharacterAction {

    public WorkingAction() : base(ACTION_TYPE.WORKING) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        WorkingAction action = new WorkingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
