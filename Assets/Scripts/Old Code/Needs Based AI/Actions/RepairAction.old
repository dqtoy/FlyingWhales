using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RepairAction : CharacterAction {
    //private int _amountToIncrease;
    //private int _resourceAmountToDecrease;
    //private StructureObj _structure;

    //#region getters/setters
    //private RESOURCE _resourceNeeded {
    //    get {
    //        if(_actionData.resourceNeeded == RESOURCE.NONE) {
    //            return _structure.madeOf;
    //        }
    //        return _actionData.resourceNeeded;
    //    }
    //}
    //#endregion

    public RepairAction() : base(ACTION_TYPE.REPAIR) {
    }

    #region Overrides
    //public override void Initialize() {
    //    base.Initialize();
    //    if (state.obj is StructureObj) {
    //        _structure = state.obj as StructureObj;
    //    }
    //    if (_amountToIncrease == 0) {
    //        _amountToIncrease = Mathf.RoundToInt((float) _structure.maxHP / (float) _actionData.duration);
    //    }
    //    if (_resourceAmountToDecrease == 0) {
    //        _resourceAmountToDecrease = Mathf.RoundToInt((float) _actionData.resourceAmountNeeded / (float) _actionData.duration);
    //    }
    //}
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if(party is CharacterParty && targetObject is StructureObj) {
            CharacterParty characterParty = party as CharacterParty;
            StructureObj structure = targetObject as StructureObj;
            int resourceAmountToDecrease = Mathf.RoundToInt((float) _actionData.resourceAmountNeeded / (float) _actionData.duration);
            int amountToIncrease = Mathf.RoundToInt((float) structure.maxHP / (float) _actionData.duration);
            RESOURCE resourceNeeded = structure.madeOf;
            if(_actionData.resourceNeeded != RESOURCE.NONE) {
                resourceNeeded = _actionData.resourceNeeded;
            }
            GiveAllReward(characterParty);

            (characterParty.characterObject as CharacterObj).AdjustResource(resourceNeeded, resourceAmountToDecrease);
            structure.AdjustHP(amountToIncrease);
            if (structure.isHPFull || (characterParty.characterObject as CharacterObj).resourceInventory[resourceNeeded] < resourceAmountToDecrease) {
                EndAction(characterParty, structure);
            }
        }
        
    }
    public override CharacterAction Clone() {
        RepairAction repairAction = new RepairAction();
        SetCommonData(repairAction);
        repairAction.Initialize();
        return repairAction;
    }
    public override bool CanBeDone(IObject targetObject) {
        if(targetObject is StructureObj) {
            StructureObj structure = targetObject as StructureObj;
            if (structure.isHPFull) {
                return false;
            }
        }
        return base.CanBeDone(targetObject);
    }
    #endregion
}
