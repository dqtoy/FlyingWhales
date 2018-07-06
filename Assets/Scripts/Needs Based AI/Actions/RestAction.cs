using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RestAction : CharacterAction {
    public RestAction() : base(ACTION_TYPE.REST) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if (party.IsFull(NEEDS.ENERGY)) {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        RestAction restAction = new RestAction();
        SetCommonData(restAction);
        restAction.Initialize();
        return restAction;
    }
    #endregion
}
