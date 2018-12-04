using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ReleaseAction : CharacterAction {
    //private CharacterObj _characterObj;
    public ReleaseAction() : base(ACTION_TYPE.RELEASE) {

    }
    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
    //    if (_state.obj.objectType == OBJECT_TYPE.CHARACTER) {
    //        _characterObj = _state.obj as CharacterObj;
    //    }
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
    }
    public override CharacterAction Clone() {
        ReleaseAction releaseAction = new ReleaseAction();
        SetCommonData(releaseAction);
        releaseAction.Initialize();
        return releaseAction;
    }
    public override void DoneDuration(Party party, IObject targetObject) {
        base.DoneDuration(party, targetObject);
        if(party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
        ReleaseCharacter(targetObject);
    }
    public override bool CanBeDone(IObject targetObject) {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
        //if (!_characterObj.character.isPrisoner) {
        //    return false;
        //}
        //return base.CanBeDone();
    }
    #endregion

    public void ReleaseCharacter(IObject targetObject) {
        if (targetObject.currentState.stateName == "Imprisoned") {
            ObjectState aliveState = targetObject.GetState("Alive");
            targetObject.ChangeState(aliveState);

            if (targetObject is ICharacterObject) {
                ICharacterObject icharacterObject = targetObject as ICharacterObject;
                icharacterObject.iparty.GoHome();
                if (icharacterObject.iparty is CharacterParty) {
                    (icharacterObject.iparty as CharacterParty).SetIsIdle(false);
                }
                
                if (icharacterObject is CharacterObj) {
                    Messenger.Broadcast(Signals.CHARACTER_RELEASED, (icharacterObject as CharacterObj).party.mainCharacter);
                }
            }
        }
    }
}
