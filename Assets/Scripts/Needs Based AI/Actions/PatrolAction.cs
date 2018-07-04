using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PatrolAction : CharacterAction {
    public PatrolAction(ObjectState state) : base(state, ACTION_TYPE.PATROL) {

    }

    #region Overrides
    public override void PerformAction(CharacterParty party) {
        base.PerformAction(party);
        ActionSuccess();
        GiveAllReward(party);
        if (party.IsFull(NEEDS.PRESTIGE)) {
            EndAction(party);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        PatrolAction patrolAction = new PatrolAction(state);
        SetCommonData(patrolAction);
        patrolAction.Initialize();
        return patrolAction;
    }
    #endregion
}