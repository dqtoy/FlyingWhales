using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DispelAction : CharacterAction {
    public DispelAction() : base(ACTION_TYPE.DISPEL) {

    }
    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        if (party.IsFull(NEEDS.FULLNESS)) {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        DispelAction dispelAction = new DispelAction();
        SetCommonData(dispelAction);
        return dispelAction;
    }
    #endregion
}
