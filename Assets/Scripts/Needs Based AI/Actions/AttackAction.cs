using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class AttackAction : CharacterAction {

    public AttackAction(ObjectState state) : base(state, ACTION_TYPE.ATTACK) {

    }
    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        ActionSuccess();
        //What happens when performing attack
    }
    public override CharacterAction Clone(ObjectState state) {
        AttackAction attackAction = new AttackAction(state);
        SetCommonData(attackAction);
        attackAction.Initialize();
        return attackAction;
    }
    #endregion
}
