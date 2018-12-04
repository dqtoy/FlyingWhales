using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DefenderAction : CharacterAction {
    public DefenderAction() : base(ACTION_TYPE.DEFENDER) {

    }


    #region Overrides
    public override CharacterAction Clone() {
        DefenderAction action = new DefenderAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
