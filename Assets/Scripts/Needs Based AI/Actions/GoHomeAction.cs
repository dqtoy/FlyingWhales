using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class GoHomeAction : CharacterAction {
    public GoHomeAction(ObjectState state) : base(state, ACTION_TYPE.GO_HOME) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveAllReward(character);
    }
    public override CharacterAction Clone(ObjectState state) {
        GoHomeAction goHomeAction = new GoHomeAction(state);
        SetCommonData(goHomeAction);
        goHomeAction.Initialize();
        return goHomeAction;
    }
    public override bool CanBeDoneBy(Character character) {
        if (character.home == null || _state.obj.objectLocation.tileLocation.areaOfTile.id != character.home.id) {
            return false;
        }
        return base.CanBeDoneBy(character);
    }
    #endregion
}
