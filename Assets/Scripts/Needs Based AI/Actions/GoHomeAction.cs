using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class GoHomeAction : CharacterAction {
    public GoHomeAction(ObjectState state) : base(state, ACTION_TYPE.GO_HOME) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
    }
    public override CharacterAction Clone(ObjectState state) {
        GoHomeAction goHomeAction = new GoHomeAction(state);
        SetCommonData(goHomeAction);
        goHomeAction.Initialize();
        return goHomeAction;
    }
    public override bool CanBeDoneBy(CharacterParty party) {
        if (party.home == null || _state.obj.objectLocation.tileLocation.areaOfTile.id != party.home.id) {
            return false;
        }
        return base.CanBeDoneBy(party);
    }
    #endregion
}
