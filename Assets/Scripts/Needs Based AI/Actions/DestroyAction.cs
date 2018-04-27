using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class DestroyAction : CharacterAction {
    private StructureObj _structure;
    private int _amountToReduce;
    public DestroyAction(ObjectState state) : base(state, ACTION_TYPE.DESTROY) {
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
    }

    #region Overrides
    public override void OnChooseAction() {
        base.OnChooseAction();
        if (_amountToReduce == 0) {
            _amountToReduce = 100 / actionData.duration;
        }
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < actionData.successRate) {
            ActionSuccess();
            GiveReward(NEEDS.PRESTIGE, character);
            GiveReward(NEEDS.ENERGY, character);
            GiveReward(NEEDS.JOY, character);

            _structure.onHPReachedZero = () => EndAction(character);
            _structure.AdjustHP(-_amountToReduce);
        } else {
            ActionFail();
            GiveReward(NEEDS.ENERGY, character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        DestroyAction destroyAction = new DestroyAction(state);
        SetCommonData(destroyAction);
        return destroyAction;
    }
    #endregion
}