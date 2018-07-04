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
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
    }
    public override CharacterAction Clone(ObjectState state) {
        ReleaseAction releaseAction = new ReleaseAction(state);
        SetCommonData(releaseAction);
        releaseAction.Initialize();
        return releaseAction;
    }
    public override void DoneDuration(CharacterParty party) {
        base.DoneDuration(party);
        GiveAllReward(party);
        ReleaseCharacter();
    }
    //public override bool CanBeDone() {
    //    if (!_characterObj.character.isPrisoner) {
    //        return false;
    //    }
    //    return base.CanBeDone();
    //}
    #endregion

    public void ReleaseCharacter() {
        if (_characterObj.currentState.stateName == "Imprisoned") {
            ObjectState aliveState = _characterObj.GetState("Alive");
            _characterObj.ChangeState(aliveState);
        }
        _characterObj.iparty.GoHome();
    }
}
