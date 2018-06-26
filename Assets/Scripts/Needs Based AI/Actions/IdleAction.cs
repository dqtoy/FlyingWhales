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
    public override bool CanBeDoneBy(Character character) {
        if(character.characterObject.objectLocation == null || character.characterObject.objectLocation.id != _state.obj.objectLocation.id || character.homeLandmark.id != _state.obj.objectLocation.id) {
            return false;
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
}
