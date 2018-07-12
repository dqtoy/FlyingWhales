using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanseAction : CharacterAction {

    public CleanseAction() : base(ACTION_TYPE.CLEANSE) {
    }

    #region Overrides
    public override CharacterAction Clone() {
        CleanseAction action = new CleanseAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
