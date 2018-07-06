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
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if(party.icharacters[0].currentCombat != null) {

        }
    }
    #endregion

    private void FindCombat() {

    }
}
