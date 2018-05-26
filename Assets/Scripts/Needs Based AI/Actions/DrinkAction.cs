using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DrinkAction : CharacterAction {
    public DrinkAction(ObjectState state) : base(state, ACTION_TYPE.DRINK) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveAllReward(character);
        if (character.role.IsFull(NEEDS.FUN)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        DrinkAction drinkAction = new DrinkAction(state);
        SetCommonData(drinkAction);
        drinkAction.Initialize();
        return drinkAction;
    }
    #endregion
}
