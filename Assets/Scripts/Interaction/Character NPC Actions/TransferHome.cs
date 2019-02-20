using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransferHome : Interaction {

    public TransferHome(Area interactable) 
        : base(interactable, INTERACTION_TYPE.TRANSFER_HOME, 0) {
        _name = "Transfer Home";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        startState.SetEffect(() => StartStateRewardEffect(startState));

        _states.Add(startState.name, startState);

        //SetCurrentState(startState);
    }
    #endregion

    private void StartStateRewardEffect(InteractionState state) {
        //BaseLandmark landmark = interactable.landmarks[UnityEngine.Random.Range(0, interactable.landmarks.Count)];
        _characterInvolved.MigrateHomeTo(interactable);

        state.AddLogFiller(new LogFiller(interactable, interactable.name, LOG_IDENTIFIER.LANDMARK_2));
    }
}
