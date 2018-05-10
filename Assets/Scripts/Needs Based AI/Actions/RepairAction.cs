using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RepairAction : CharacterAction {
    private int _amountToIncrease;
    private int _resourceAmountToDecrease;
    private RESOURCE _resourceNeeded;
    private StructureObj _structure;

    public RepairAction(ObjectState state) : base(state, ACTION_TYPE.REPAIR) {
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
            _resourceNeeded = _actionData.resourceNeeded;
            if (_resourceNeeded == RESOURCE.NONE) {
                _resourceNeeded = _structure.madeOf;
            }
        }
        if (_amountToIncrease == 0) {
            _amountToIncrease = Mathf.RoundToInt(100f / (float) _actionData.duration);
        }
        if (_resourceAmountToDecrease == 0) {
            _resourceAmountToDecrease = Mathf.RoundToInt((float) _actionData.resourceAmountNeeded / (float) _actionData.duration);
        }
        
    }

    #region Overrides
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);

        character.characterObject.AdjustResource(_resourceNeeded, _resourceAmountToDecrease);
        _structure.AdjustHP(_amountToIncrease);
        if (_structure.isHPFull || character.characterObject.resourceInventory[_resourceNeeded] < _resourceAmountToDecrease) {
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
