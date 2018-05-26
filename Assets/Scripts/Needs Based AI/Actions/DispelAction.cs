using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DispelAction : CharacterAction {
    public DispelAction(ObjectState state) : base(state, ACTION_TYPE.DISPEL) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveAllReward(character);
        if (character.role.IsFull(NEEDS.FULLNESS)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        EatAction eatAction = new EatAction(state);
        SetCommonData(eatAction);
        return eatAction;
    }
    #endregion
}
