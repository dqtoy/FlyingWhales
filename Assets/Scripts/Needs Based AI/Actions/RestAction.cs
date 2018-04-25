using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RestAction : CharacterAction {
    public RestAction(ObjectState state) : base(state, ACTION_TYPE.REST) {

    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);
        if (character.role.IsFull(NEEDS.ENERGY)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone() {
        RestAction restAction = new RestAction(_state);
        SetCommonData(restAction);
        return restAction;
    }
    #endregion
}
