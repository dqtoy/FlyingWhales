using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAction : CharacterAction {

    protected int cooldown;

    public TrainAction() : base(ACTION_TYPE.TRAIN) {
    }

    #region Overrides
    public override CharacterAction Clone() {
        TrainAction idleAction = new TrainAction();
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    public override bool CanBeDone(IObject targetObject) {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    public override bool CanBeDoneBy(CharacterParty party, IObject targetObject) {
        if (cooldown != 0) {
            return false; //action has not yet cooled down
        }
        return true;
    }
    #endregion
}
