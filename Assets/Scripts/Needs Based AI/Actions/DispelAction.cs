using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DispelAction : CharacterAction {
    public DispelAction(ObjectState state) : base(state, ACTION_TYPE.DISPEL) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
        if (party.IsFull(NEEDS.FULLNESS)) {
            EndAction(party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        EatAction eatAction = new EatAction(state);
        SetCommonData(eatAction);
        return eatAction;
    }
    #endregion
}
