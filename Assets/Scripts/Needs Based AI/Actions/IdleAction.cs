using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class IdleAction : CharacterAction {
    public IdleAction(ObjectState state) : base(state, ACTION_TYPE.IDLE) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveAllReward(character);
    }
    public override CharacterAction Clone(ObjectState state) {
        IdleAction idleAction = new IdleAction(state);
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    #endregion
}
