using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfMutilateAction : CharacterAction {
    public SelfMutilateAction() : base(ACTION_TYPE.SELFMUTILATE) {
        _actionData.providedFun = -1f;
        _actionData.providedSanity = -1f;
        _actionData.duration = 8;
    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if(party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        Mutilate(party);
    }
    public override CharacterAction Clone() {
        SelfMutilateAction action = new SelfMutilateAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion

    private void Mutilate(Party party) {
        int hpReduction = (int)((float)party.mainCharacter.maxHP * 0.02f);
        //party.mainCharacter.AdjustHP(-hpReduction);
    }
}