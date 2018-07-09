using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlailAction : CharacterAction {

    public FlailAction() : base(ACTION_TYPE.FLAIL) {
        _actionData.providedFun = -0.1f;
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
