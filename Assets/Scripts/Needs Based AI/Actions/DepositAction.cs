using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DepositAction : CharacterAction {
    public DepositAction() : base(ACTION_TYPE.DEPOSIT) {
        _actionData.providedPrestige = 5f;
    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);

    }
    public override CharacterAction Clone() {
        DepositAction depositAction = new DepositAction();
        SetCommonData(depositAction);
        depositAction.Initialize();
        return depositAction;
    }
    #endregion
}
