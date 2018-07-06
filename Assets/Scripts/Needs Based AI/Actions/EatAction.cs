using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class EatAction : CharacterAction {
    public EatAction() : base(ACTION_TYPE.EAT) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if (party.IsFull(NEEDS.FULLNESS)) {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        EatAction eatAction = new EatAction();
        SetCommonData(eatAction);
        eatAction.Initialize();
        return eatAction;
    }
    #endregion
}
