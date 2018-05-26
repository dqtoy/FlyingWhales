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
        GiveAllReward(character);
        if (character.role.IsFull(NEEDS.ENERGY)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        RestAction restAction = new RestAction(state);
        SetCommonData(restAction);
        restAction.Initialize();
        return restAction;
    }
    #endregion
}
