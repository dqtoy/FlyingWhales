using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToReturnHome : Interaction {

    private Area targetLocation;

    public MoveToReturnHome(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.MOVE_TO_RETURN_HOME, 0) {
        _name = "Move To Return Home";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        targetLocation = _characterInvolved.homeLandmark.tileLocation.areaOfTile;

        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        startState.SetEffect(() => StartStateRewardEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    private void StartStateRewardEffect(InteractionState state) {
        //**Mechanics**: Character will start its travel to home location
        _characterInvolved.ownParty.GoHomeAndDisband();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
}
