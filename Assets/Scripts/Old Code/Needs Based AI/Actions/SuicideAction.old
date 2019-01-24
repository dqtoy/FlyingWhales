
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuicideAction : CharacterAction {
    public SuicideAction() : base(ACTION_TYPE.SUICIDE) {
    }

    #region overrides
    public override CharacterAction Clone() {
        SuicideAction action = new SuicideAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        Debug.Log(GameManager.Instance.TodayLogString() + party.owner.name + " committed suicide!");
        party.owner.Death(); //this wll also end the action
    }
    public override bool ShouldGoToTargetObjectOnChoose() {
        return false;
    }
    #endregion
}
