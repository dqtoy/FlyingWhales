using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanResidences : StructureObj {

    public HumanResidences() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.HUMAN_RESIDENCES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        HumanResidences clone = new HumanResidences();
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
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        LandmarkData data = LandmarkManager.Instance.GetLandmarkData(newLocation.specificLandmarkType);
        int numOfCivilians = UnityEngine.Random.Range(data.minCivilians, data.maxCivilians + 1);
        _resourceInventory[RESOURCE.HUMAN_CIVILIAN] = numOfCivilians;
    }
    #endregion
}