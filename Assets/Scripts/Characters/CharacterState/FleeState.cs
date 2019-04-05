using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleeState : CharacterState {

    public FleeState(CharacterStateComponent characterComp) : base(characterComp) {
        stateName = "Flee State";
        characterState = CHARACTER_STATE.FLEE;
        stateCategory = CHARACTER_STATE_CATEGORY.MINOR;
        //duration = 288;
    }

    #region Overrides
    protected override void DoMovementBehavior() {
        base.DoMovementBehavior();
        StartFleeMovement();
    }
    protected override void PerTickInState() {
        base.PerTickInState();
        stateComponent.character.marker.RedetermineFlee(); 
    }
    #endregion

    private void StartFleeMovement() {
        stateComponent.character.marker.OnStartFlee();
    }

    public void CheckForEndState() {
        if (stateComponent.character.marker.hostilesInRange.Count == 0) {
            //can end flee
            OnExitThisState();
        } else {
            //redetermine flee path
            stateComponent.character.marker.RedetermineFlee();
        }
    }
}
