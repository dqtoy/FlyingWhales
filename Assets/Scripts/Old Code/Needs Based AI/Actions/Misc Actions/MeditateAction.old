using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeditateAction : CharacterAction {

    public MeditateAction() : base(ACTION_TYPE.MEDITATE) {
        _actionData.providedEnergy = -1f;
        _actionData.providedFun = 1f;

        _actionData.duration = 8;
        _weight = 200;
    }

    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);

        //give the character the Provided Hunger, Provided Energy, Provided Joy, Provided Prestige
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override CharacterAction Clone() {
        MeditateAction action = new MeditateAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
