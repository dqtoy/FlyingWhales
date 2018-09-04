using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartyAction : CharacterAction {
    public PartyAction() : base(ACTION_TYPE.PARTY) { }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        PartyAction action = new PartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
