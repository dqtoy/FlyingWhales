using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatAction : CharacterAction {

    public ChatAction() : base(ACTION_TYPE.CHAT) {
        _actionData.providedSanity = 3f;

        _actionData.duration = 12;
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        ChatAction action = new ChatAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
