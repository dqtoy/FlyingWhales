using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaydreamAction : CharacterAction {

    public DaydreamAction() : base(ACTION_TYPE.DAYDREAM) {
        _actionData.providedFullness = -0.2f;
        _actionData.providedEnergy = -0.2f;
        _actionData.providedFun = -0.2f;
        _actionData.providedPrestige = -0.2f;

        _actionData.duration = 48;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    #endregion
}
