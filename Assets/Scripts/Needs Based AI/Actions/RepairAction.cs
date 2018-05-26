using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RepairAction : CharacterAction {
    private int _amountToIncrease;
    private int _resourceAmountToDecrease;
    private StructureObj _structure;

    #region getters/setters
    private RESOURCE _resourceNeeded {
        get {
            if(_actionData.resourceNeeded == RESOURCE.NONE) {
                return _structure.madeOf;
            }
            return _actionData.resourceNeeded;
        }
    }
    #endregion

    public RepairAction(ObjectState state) : base(state, ACTION_TYPE.REPAIR) {
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        if (state.obj is StructureObj) {
            _structure = state.obj as StructureObj;
        }
        if (_amountToIncrease == 0) {
            _amountToIncrease = Mathf.RoundToInt((float) _structure.maxHP / (float) _actionData.duration);
        }
        if (_resourceAmountToDecrease == 0) {
            _resourceAmountToDecrease = Mathf.RoundToInt((float) _actionData.resourceAmountNeeded / (float) _actionData.duration);
        }
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        GiveAllReward(character);

        character.characterObject.AdjustResource(_resourceNeeded, _resourceAmountToDecrease);
        _structure.AdjustHP(_amountToIncrease);
        if (_structure.isHPFull || character.characterObject.resourceInventory[_resourceNeeded] < _resourceAmountToDecrease) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        RepairAction repairAction = new RepairAction(state);
        SetCommonData(repairAction);
        repairAction.Initialize();
        return repairAction;
    }
    #endregion
}
