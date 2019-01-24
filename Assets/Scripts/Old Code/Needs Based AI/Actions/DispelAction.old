using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DispelAction : CharacterAction {
    public DispelAction() : base(ACTION_TYPE.DISPEL) {

    }
    #region Overrides
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            CharacterParty characterParty = party as CharacterParty;
            GiveAllReward(characterParty);
            if (characterParty.IsFull(NEEDS.FULLNESS)) {
                EndAction(characterParty, targetObject);
            }
        }
    }
    public override CharacterAction Clone() {
        DispelAction dispelAction = new DispelAction();
        SetCommonData(dispelAction);
        return dispelAction;
    }
    #endregion
}
