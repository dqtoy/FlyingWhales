using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HousekeepingAction : CharacterAction {

    public HousekeepingAction() : base(ACTION_TYPE.HOUSEKEEPING) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        HousekeepingAction action = new HousekeepingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
