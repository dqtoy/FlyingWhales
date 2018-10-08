using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkAction : CharacterAction {

    public BerserkAction() : base(ACTION_TYPE.BERSERK) {
        _actionData.providedEnergy = -2f;
        _actionData.providedSanity = 1f;

        _actionData.duration = 12;
    }

    #region Overrides
    public override void OnChooseAction(Party iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        iparty.BerserkModeOn();
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
    }
    public override void EndAction(Party party, IObject targetObject) {
        base.EndAction(party, targetObject);
        party.BerserkModeOff();
    }
    public override CharacterAction Clone() {
        BerserkAction action = new BerserkAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
