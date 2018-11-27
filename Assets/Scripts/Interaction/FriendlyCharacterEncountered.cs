using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class FriendlyCharacterEncountered : Interaction {

    private Character _targetCharacter;
    
    public FriendlyCharacterEncountered(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.FRIENDLY_CHARACTER_ENCOUNTERED, 70) {
        _name = "Friendly Character Encountered";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterRecruitedState = new InteractionState("Character Recruited", this);
        InteractionState nothingHappenedState = new InteractionState("Nothing Happened", this);

        //startState.SetEndEffect(() => StartEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion
}
