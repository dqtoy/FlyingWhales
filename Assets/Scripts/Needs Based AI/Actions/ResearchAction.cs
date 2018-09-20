using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchAction : CharacterAction {
    public ResearchAction() : base(ACTION_TYPE.RESEARCH) {
    }

    #region overrides
    public override CharacterAction Clone() {
        ResearchAction action = new ResearchAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
    }
    #endregion
}
