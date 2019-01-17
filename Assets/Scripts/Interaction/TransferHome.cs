using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferHome : Interaction {

    public TransferHome(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.TRANSFER_HOME, 0) {
        _name = "Transfer Home";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(interactable.tileLocation.areaOfTile, interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        startState.SetEffect(() => StartStateRewardEffect(startState));

        _states.Add(startState.name, startState);

        SetCurrentState(startState);
    }
    #endregion

    private void StartStateRewardEffect(InteractionState state) {
        BaseLandmark landmark = interactable.tileLocation.areaOfTile.landmarks[UnityEngine.Random.Range(0, interactable.tileLocation.areaOfTile.landmarks.Count)];
        _characterInvolved.MigrateTo(landmark);

        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile, interactable.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_2));
    }
}
