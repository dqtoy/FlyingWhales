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
        ReleaseAction releaseAction = new ReleaseAction(state);
        SetCommonData(releaseAction);
        releaseAction.Initialize();
        return releaseAction;
    }
    public override void DoneDuration() {
        base.DoneDuration();
        //Go home: Get home tile > get house structure obj > Get go home action from state > assign action
    }
    public override bool CanBeDone() {
        if (!_characterObj.character.isPrisoner) {
            return false;
        }
        return base.CanBeDone();
    }
    #endregion
}
