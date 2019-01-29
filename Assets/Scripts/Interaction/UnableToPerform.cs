using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnableToPerform : Interaction {

    private string _unableToPerformActionName; 

    public UnableToPerform(Area interactable) 
        : base(interactable, INTERACTION_TYPE.UNABLE_TO_PERFORM, 0) {
        _name = "Unable To Perform";
        _unableToPerformActionName = string.Empty;
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
        state.AddLogFiller(new LogFiller(null, _unableToPerformActionName, LOG_IDENTIFIER.STRING_1));
    }

    public void SetActionNameThatCannotBePerformed(string interactionName) {
        _unableToPerformActionName = interactionName;
    }
}
