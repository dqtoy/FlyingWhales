using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfMutilateAction : CharacterAction {
    public SelfMutilateAction() : base(ACTION_TYPE.SELFMUTILATE) {
        _actionData.providedFun = -0.3f;
        _actionData.providedEnergy = -0.3f;
        _actionData.duration = 48;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        Mutilate(party);
    }
    #endregion

    private void Mutilate(NewParty party) {
        int hpReduction = (int)((float)party.icharacters[0].maxHP * 0.02f);
        party.icharacters[0].AdjustHP(-hpReduction);
    }
}