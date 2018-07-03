using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garrison : StructureObj {

    //public int cooldown;

    public int armyStrength { //Army Strength is 25% of the linked Settlement's Civilian Count
        get { return this.objectLocation.tileLocation.areaOfTile == null ? 0 : (int)(this.objectLocation.tileLocation.areaOfTile.totalCivilians * 0.25f); }
    }

    public Garrison() {
        _specificObjectType = LANDMARK_TYPE.GARRISON;
        SetObjectName(Utilities.NormalizeStringUpperCaseFirstLetters(_specificObjectType.ToString()));
    }

    #region Overrides
    public override IObject Clone() {
        Garrison clone = new Garrison();
        SetCommonData(clone);
        return clone;
    }
    public override void StartState(ObjectState state) {
        base.StartState(state);
        if (state.stateName == "Corrupted") { //When state changes to Corrupted, add 100 Corruption Value.
            //ScheduleDoneTraining();
        }else if (state.stateName == "Training") {
            ScheduleDoneTraining();
        }
    }
    #endregion

    #region Utilities
    private void ScheduleDoneTraining() {
        GameDate readyDate = GameManager.Instance.Today();
        readyDate.AddHours(336); // 1 week
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
