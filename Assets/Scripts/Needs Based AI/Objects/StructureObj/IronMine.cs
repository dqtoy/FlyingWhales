using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronMine : StructureObj {

	public IronMine() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.IRON_MINE;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _resourceInventory[RESOURCE.IRON] = 5000;
    }

    #region Overrides
    public override IObject Clone() {
        IronMine clone = new IronMine();
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
                ObjectState emptyState = GetState("Depleted");
                ChangeState(emptyState);
            }
        } else {
            if (_currentState.stateName == "Depleted") {
                ObjectState occupiedState = GetState("Default");
                ChangeState(occupiedState);
            }
        }
    }
    #endregion
}
