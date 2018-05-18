using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OakLumberyard : StructureObj {

    public OakLumberyard() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.OAK_LUMBERYARD;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _resourceInventory[RESOURCE.OAK] = 5000; //Initial amount should be 5000
    }

    #region Overrides
    public override IObject Clone() {
        OakLumberyard clone = new OakLumberyard();
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
