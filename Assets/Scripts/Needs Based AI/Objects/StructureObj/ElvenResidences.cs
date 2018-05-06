using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElvenResidences : StructureObj {

    public ElvenResidences() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.ELVEN_RESIDENCES;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        ElvenResidences clone = new ElvenResidences();
        clone.SetObjectName(this._objectName);
        clone._specificObjectType = this._specificObjectType;
        clone._objectType = this._objectType;
        clone._isInvisible = this.isInvisible;
        clone._maxHP = this.maxHP;
        clone._onHPReachedZero = this._onHPReachedZero;
        clone._onHPReachedFull = this._onHPReachedFull;
        List<ObjectState> states = new List<ObjectState>();
        for (int i = 0; i < this.states.Count; i++) {
            ObjectState currState = this.states[i];
            ObjectState clonedState = currState.Clone(clone);
            states.Add(clonedState);
            //if (this.currentState == currState) {
            //    clone.ChangeState(clonedState);
            //}
        }
        clone.SetStates(states);
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