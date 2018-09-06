using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FetchAction : CharacterAction {
    public FetchAction() : base(ACTION_TYPE.FETCH) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //TODO: Add Item Obtaining from monster drops
        //GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        FetchAction action = new FetchAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
