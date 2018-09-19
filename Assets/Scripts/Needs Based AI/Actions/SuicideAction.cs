using ECS;
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
    public override void OnChooseAction(NewParty iparty, IObject targetObject) {
        base.OnChooseAction(iparty, targetObject);
        Debug.Log(GameManager.Instance.TodayLogString() + iparty.owner.name + " committed suicide!");
        iparty.owner.Death();
    }
    public override bool ShouldGoToTargetObjectOnChoose() {
        return false;
    }
    #endregion
}
