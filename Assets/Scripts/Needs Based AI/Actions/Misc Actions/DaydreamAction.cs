using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaydreamAction : CharacterAction {

    public DaydreamAction() : base(ACTION_TYPE.DAYDREAM) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        DaydreamAction action = new DaydreamAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
