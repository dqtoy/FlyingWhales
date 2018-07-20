using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InPartyAction : CharacterAction {
    public InPartyAction() : base(ACTION_TYPE.IN_PARTY) {
    }

    #region overrides
    public override CharacterAction Clone() {
        InPartyAction action = new InPartyAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    #endregion
}
