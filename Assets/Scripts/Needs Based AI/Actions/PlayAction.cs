using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAction : CharacterAction {

    public PlayAction() : base(ACTION_TYPE.PLAY) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 2f;

        _actionData.duration = 24;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        PlayAction action = new PlayAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
