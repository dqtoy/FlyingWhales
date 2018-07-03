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
        //if (character.characterObject.objectLocation == null || character.characterObject.objectLocation.id != _state.obj.objectLocation.id || character.homeStructure.objectLocation.id != _state.obj.objectLocation.id) {
        if (character.characterObject.objectLocation == null || character.homeStructure.objectLocation.id != _state.obj.objectLocation.id) {
            //the characters location is null or the object that this action belongs to is not the home of the character
            return false;
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
}
