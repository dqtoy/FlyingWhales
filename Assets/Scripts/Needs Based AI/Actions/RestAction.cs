using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RestAction : CharacterAction {
    public RestAction(ObjectState state) : base(state, ACTION_TYPE.REST) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
        if (party.IsFull(NEEDS.ENERGY)) {
            EndAction(party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        RestAction restAction = new RestAction(state);
        SetCommonData(restAction);
        restAction.Initialize();
        return restAction;
    }
    #endregion
}
