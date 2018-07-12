using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfMutilateAction : CharacterAction {
    public SelfMutilateAction() : base(ACTION_TYPE.SELFMUTILATE) {
        _actionData.providedFun = -1f;
        _actionData.providedSanity = -1f;
        _actionData.duration = 24;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        Mutilate(party);
    }
    public override CharacterAction Clone() {
        SelfMutilateAction action = new SelfMutilateAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    private void Mutilate(NewParty party) {
        int hpReduction = (int)((float)party.icharacters[0].maxHP * 0.01f);
        party.icharacters[0].AdjustHP(-hpReduction);
    }
}