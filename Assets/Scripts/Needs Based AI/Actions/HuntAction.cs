using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class HuntAction : CharacterAction {
    public HuntAction(): base(ACTION_TYPE.HUNT) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        int chance = UnityEngine.Random.Range(0, 100);
        if(chance < actionData.successRate) {
            ActionSuccess(targetObject);
            GiveAllReward(party);
            if (party.IsFull(NEEDS.FULLNESS)){
                EndAction(party, targetObject);
            }
        } else {
            ActionFail(targetObject);
            GiveReward(NEEDS.ENERGY, party);
        }
    }
    public override CharacterAction Clone() {
        HuntAction huntAction = new HuntAction();
        SetCommonData(huntAction);
        huntAction.Initialize();
        return huntAction;
    }
    #endregion
}