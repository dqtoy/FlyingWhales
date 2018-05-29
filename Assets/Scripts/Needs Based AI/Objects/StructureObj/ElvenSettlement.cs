using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElvenSettlement : StructureObj {

    public ElvenSettlement() : base() {
        _specificObjectType = SPECIFIC_OBJECT_TYPE.ELVEN_SETTLEMENT;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
        _resourceInventory[RESOURCE.ELF_CIVILIAN] = 5000;
    }

    #region Overrides
    public override IObject Clone() {
        ElvenSettlement clone = new ElvenSettlement();
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
    public override void StartState(ObjectState state) {
        base.StartState(state);
        if (state.stateName == "Training") {
            ScheduleDoneTraining();
        }
    }
    public override void OnAddToLandmark(BaseLandmark newLocation) {
        base.OnAddToLandmark(newLocation);
        this.objectLocation.landmarkObject.SetIconActive(true);
    }
    #endregion

    #region Resource Inventory
    public override RESOURCE GetMainResource() {
        return RESOURCE.ELF_CIVILIAN;
    }
    #endregion

    #region Utilities
    private void ScheduleDoneTraining() {
        GameDate readyDate = GameManager.Instance.Today();
        readyDate.AddDays(30);
        SchedulingManager.Instance.AddEntry(readyDate, DoneTraining);
    }
    private void DoneTraining() {
        if (_currentState.stateName == "Training") {
            ObjectState readyState = GetState("Ready");
            ChangeState(readyState);
        }
    }
    public void CommenceTraining() {
        if (_currentState.stateName == "Ready") {
            ObjectState trainingState = GetState("Training");
            ChangeState(trainingState);
        }
    }
    #endregion
}