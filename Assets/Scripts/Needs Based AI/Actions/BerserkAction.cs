using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BerserkAction : CharacterAction {

    public BerserkAction() : base(ACTION_TYPE.BERSERK) {
        _actionData.providedFullness = -0.2f;
        _actionData.providedEnergy = -0.2f;
        _actionData.providedFun = -0.2f;
        _actionData.providedPrestige = -0.2f;

        _actionData.duration = 48;
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
    #endregion
}
