using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PrayAction : CharacterAction {

    public PrayAction() : base(ACTION_TYPE.PRAY) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        //if (party.IsFull(NEEDS.SANITY)) {
        //    EndAction(party, targetObject);
        //}
    }
    public override CharacterAction Clone() {
        PrayAction prayAction = new PrayAction();
        SetCommonData(prayAction);
        prayAction.Initialize();
        return prayAction;
    }
    #endregion
}
