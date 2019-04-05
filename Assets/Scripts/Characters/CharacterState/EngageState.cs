using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngageState : CharacterState {

    public EngageState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Engage State";
        characterState = CHARACTER_STATE.ENGAGE;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        //duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartEngageMovement();
    }
    protected override void PerTickInState() {
        base.PerTickInState();
        stateComponent.character.marker.RedetermineEngage();
    }
    #endregion

    private void StartEngageMovement() {
        stateComponent.character.marker.OnStartEngage();
    }

    public void CheckForEndState() {
        if (stateComponent.character.marker.hostilesInRange.Count == 0) {
            //can end engage
            stateComponent.ExitCurrentState(this);
        } else {
            //redetermine flee path
            stateComponent.character.marker.RedetermineEngage();
        }
    }
}
