using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class EatAction : CharacterAction {
    public EatAction(ObjectState state) : base(state, ACTION_TYPE.EAT) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);
        if (character.role.IsFull(NEEDS.FULLNESS)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        EatAction eatAction = new EatAction(state);
        SetCommonData(eatAction);
        eatAction.Initialize();
        return eatAction;
    }
    #endregion
}
