using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlailAction : CharacterAction {

    public FlailAction() : base(ACTION_TYPE.FLAIL) {
        _actionData.providedEnergy = -2f;
        _actionData.providedFullness = -1f;
        _actionData.duration = 12;
    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override CharacterAction Clone() {
        FlailAction action = new FlailAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
