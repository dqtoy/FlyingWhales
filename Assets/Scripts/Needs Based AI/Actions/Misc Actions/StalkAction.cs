using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StalkAction : CharacterAction {

    public StalkAction() : base(ACTION_TYPE.STALK) {
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
        StalkAction action = new StalkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
