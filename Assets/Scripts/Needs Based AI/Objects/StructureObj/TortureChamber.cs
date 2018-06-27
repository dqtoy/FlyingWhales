using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TortureChamber : StructureObj {

	public TortureChamber() : base() {
        //_specificObjectType = SPECIFIC_OBJECT_TYPE.TORTURE_CHAMBER;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        TortureChamber clone = new TortureChamber();
        SetCommonData(clone);
        return clone;
    }
    public override void AdjustResource(RESOURCE resource, int amount) {
        _resourceInventory[resource] += amount;
        if (_resourceInventory[resource] < 0) {
            _resourceInventory[resource] = 0;
        }
        if (this.objectLocation.civilianCount == 0) {
            if (_currentState.stateName == "Occupied") {
                ObjectState emptyState = GetState("Empty");
                ChangeState(emptyState);
            }
        } else {
            if (_currentState.stateName == "Empty") {
                ObjectState occupiedState = GetState("Occupied");
                ChangeState(occupiedState);
            }
        }
    }
    #endregion
}
