using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class EatAction : CharacterAction {
    public EatAction(ObjectState state) : base(state, ACTION_TYPE.EAT) {

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
        eatAction.Initialize();
        return eatAction;
    }
    #endregion
}
