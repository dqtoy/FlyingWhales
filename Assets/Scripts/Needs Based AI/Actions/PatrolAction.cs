using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PatrolAction : CharacterAction {
    public PatrolAction() : base(ACTION_TYPE.PATROL) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        GiveAllReward(party);
        //if (party.IsFull(NEEDS.PRESTIGE)) {
            EndAction(party, targetObject);
        //}
    }
    public override CharacterAction Clone() {
        PatrolAction patrolAction = new PatrolAction();
        SetCommonData(patrolAction);
        patrolAction.Initialize();
        return patrolAction;
    }
    #endregion
}