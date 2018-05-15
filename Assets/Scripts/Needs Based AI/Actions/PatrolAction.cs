using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class PatrolAction : CharacterAction {
    public PatrolAction(ObjectState state) : base(state, ACTION_TYPE.PATROL) {

    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);
        if (character.role.IsFull(NEEDS.PRESTIGE)) {
            EndAction(character);
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