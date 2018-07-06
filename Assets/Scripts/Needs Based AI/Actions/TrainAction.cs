using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainAction : CharacterAction {
    public TrainAction(ObjectState state) : base(state, ACTION_TYPE.TRAIN) {
    }

    #region Overrides
    public override CharacterAction Clone(ObjectState state) {
        TrainAction idleAction = new TrainAction(state);
        SetCommonData(idleAction);
        idleAction.Initialize();
        return idleAction;
    }
    public override bool CanBeDone() {
        return false; //Change this to something more elegant, this is to prevent other characters that don't have the release character quest from releasing this character.
    }
    #endregion
}
