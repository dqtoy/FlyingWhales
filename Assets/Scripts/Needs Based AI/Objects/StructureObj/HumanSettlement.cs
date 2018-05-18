using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSettlement : StructureObj {

    public HumanSettlement() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.HUMAN_SETTLEMENT;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _resourceInventory[RESOURCE.HUMAN_CIVILIAN] = 500;
    }

    #region Overrides
    public override IObject Clone() {
        HumanSettlement clone = new HumanSettlement();
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
    //public override void OnAddToLandmark(BaseLandmark newLocation) {
    //    base.OnAddToLandmark(newLocation);
    //    LandmarkData data = LandmarkManager.Instance.GetLandmarkData(newLocation.specificLandmarkType);
    //    //int numOfCivilians = UnityEngine.Random.Range(data.minCivilians, data.maxCivilians + 1);
    //    //_resourceInventory[RESOURCE.HUMAN_CIVILIAN] = numOfCivilians;
    //}
    #endregion

    #region Resource Inventory
    public override RESOURCE GetMainResource() {
        return RESOURCE.HUMAN_CIVILIAN;
    }
    #endregion
}