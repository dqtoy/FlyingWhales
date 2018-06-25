using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ReleaseAction : CharacterAction {
    private CharacterObj _characterObj;
    public ReleaseAction(ObjectState state) : base(state, ACTION_TYPE.RELEASE) {

    }
    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
            _characterObj = _state.obj as CharacterObj;
        }
    }
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
    public override bool CanBeDone() {
        if (!(_characterObj.character is ECS.Character)) {
            if (!(_characterObj.character as ECS.Character).isPrisoner) {
                return false;
            }
        }
        
        return base.CanBeDone();
    }
    #endregion
}
