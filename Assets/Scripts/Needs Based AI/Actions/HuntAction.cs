using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class HuntAction : CharacterAction {
    public HuntAction(ObjectState state): base(state, ACTION_TYPE.HUNT) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < actionData.successRate) {
            ActionSuccess();
            GiveAllReward(party);
            if (party.IsFull(NEEDS.FULLNESS)){
                EndAction(party);
            }
        } else {
            ActionFail();
            GiveReward(NEEDS.ENERGY, party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        HuntAction huntAction = new HuntAction(state);
        SetCommonData(huntAction);
        huntAction.Initialize();
        return huntAction;
    }
    #endregion
}