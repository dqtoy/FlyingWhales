using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RepairAction : CharacterAction {
    private int _amountToIncrease;
    private StructureObj _structure;
    public RepairAction(ObjectState state) : base(state, ACTION_TYPE.REPAIR) {
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
    }

    #region Overrides
    public override void OnChooseAction() {
        base.OnChooseAction();
        if (_amountToIncrease == 0) {
            _amountToIncrease = Mathf.RoundToInt(100f / (float)_actionData.duration);
        }
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);

        //TODO: Resources

        _structure.AdjustHP(_amountToIncrease);
        if (_structure.isHPFull) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        RepairAction repairAction = new RepairAction(state);
        SetCommonData(repairAction);
        return repairAction;
    }
    #endregion
}
