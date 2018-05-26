using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class DestroyAction : CharacterAction {
    private StructureObj _structure;
    private int _amountToReduce;
    public DestroyAction(ObjectState state) : base(state, ACTION_TYPE.DESTROY) {
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
        if (_amountToReduce == 0) {
            _amountToReduce = Mathf.RoundToInt((float) _structure.maxHP / (float) _actionData.duration);
        }
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        if (_structure.isHPZero) {
            EndAction(character);
            return;
        }
        int chance = UnityEngine.Random.Range(0, 100);
        if (chance < actionData.successRate) {
            ActionSuccess();
            GiveAllReward(character);

            _structure.AdjustHP(-_amountToReduce);
            if (_structure.isHPZero) {
                EndAction(character);
            }
        } else {
            ActionFail();
            GiveReward(NEEDS.ENERGY, character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        DestroyAction destroyAction = new DestroyAction(state);
        SetCommonData(destroyAction);
        destroyAction.Initialize();
        return destroyAction;
    }
    #endregion
}