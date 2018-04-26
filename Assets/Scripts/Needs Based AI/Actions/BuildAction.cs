using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildAction : CharacterAction {
    private StructureObj _structure;
    private int _amountToIncrease;
    public BuildAction(ObjectState state) : base(state, ACTION_TYPE.BUILD) {
        if (_state.obj is StructureObj) {
            _structure = _state.obj as StructureObj;
        }
        _amountToIncrease = 100 / _actionData.duration;
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        //TODO: this function
        ActionSuccess();
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);
        if (character.role.IsFull(NEEDS.ENERGY)) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        BuildAction buildAction = new BuildAction(state);
        SetCommonData(buildAction);
        return buildAction;
    }
    #endregion
}
