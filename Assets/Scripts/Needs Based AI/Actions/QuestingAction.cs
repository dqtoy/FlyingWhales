using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestingAction : CharacterAction {
    public QuestingAction() : base(ACTION_TYPE.QUESTING) { }

    #region overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        GiveAllReward(party);
    }
    public override CharacterAction Clone() {
        QuestingAction action = new QuestingAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
