using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeQuestAction : CharacterAction {

    public TakeQuestAction() : base(ACTION_TYPE.TAKE_QUEST) {
    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        TakeQuestAction action = new TakeQuestAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
