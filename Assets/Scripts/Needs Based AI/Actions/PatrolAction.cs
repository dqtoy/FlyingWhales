using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PatrolAction : CharacterAction {
    public PatrolAction() : base(ACTION_TYPE.PATROL) {

    }

    #region Overrides
    public override void PerformAction(NewParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }
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