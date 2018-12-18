using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnableToPerform : Interaction {

    public UnableToPerform(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.UNABLE_TO_PERFORM, 0) {
        _name = "Unable To Perform";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        startState.SetEffect(() => StartStateRewardEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    private void StartStateRewardEffect(InteractionState state) {

    }
}
