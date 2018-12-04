using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PrayAction : CharacterAction {

    public PrayAction() : base(ACTION_TYPE.PRAY) {

    }
    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
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
