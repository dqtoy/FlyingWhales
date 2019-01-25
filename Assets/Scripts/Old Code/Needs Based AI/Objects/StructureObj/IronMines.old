using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronMines : StructureObj {

	public IronMines() : base() {
        _specificObjectType = LANDMARK_TYPE.IRON_MINES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        IronMines clone = new IronMines();
        SetCommonData(clone);
        return clone;
    }
    public override void AdjustResource(RESOURCE resource, int amount) {
        _resourceInventory[resource] += amount;
        if (_resourceInventory[resource] < 0) {
            _resourceInventory[resource] = 0;
        }
        if (_resourceInventory[resource] == 0) {
            if (_currentState.stateName == "Default") {
                ObjectState emptyState = GetState("Empty");
                ChangeState(emptyState);
            }
        } else {
            if (_currentState.stateName == "Empty") {
                ObjectState occupiedState = GetState("Default");
                ChangeState(occupiedState);
            }
        }
    }
    #endregion
}
