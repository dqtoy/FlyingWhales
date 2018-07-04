using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PrayAction : CharacterAction {

    public PrayAction(ObjectState state) : base(state, ACTION_TYPE.PRAY) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
        if (party.IsFull(NEEDS.SANITY)) {
            EndAction(party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        PrayAction prayAction = new PrayAction(state);
        SetCommonData(prayAction);
        prayAction.Initialize();
        return prayAction;
    }
    #endregion
}
