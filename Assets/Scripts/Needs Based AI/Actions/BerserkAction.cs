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
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        iparty.BerserkModeOn();
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
    }
    public override void EndAction(CharacterParty party, IObject targetObject) {
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
