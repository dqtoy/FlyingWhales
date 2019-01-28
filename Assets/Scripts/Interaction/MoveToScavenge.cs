using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToScavenge : Interaction {

    private const string Scavenge_Cancelled = "Scavenge Cancelled";
    private const string Scavenge_Proceeds = "Scavenge Proceeds";
    private const string Normal_Scavenge = "Normal Scavenge";

    private Area targetArea;

    public MoveToScavenge(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_SCAVENGE, 70) {
        _name = "Move To Scavenge";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetTargetArea(Area target) {
        targetArea = target;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState scavengeCancelled = new InteractionState(Scavenge_Cancelled, this);
        InteractionState scavengeProceeds = new InteractionState(Scavenge_Proceeds, this);
        InteractionState normalScavenge = new InteractionState(Normal_Scavenge, this);

        if (targetArea == null) {
            targetArea = GetTargetArea();
        }
        AddToDebugLog("Set target area to " + targetArea.name);
        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        scavengeCancelled.SetEffect(() => ScavengeCancelledRewardEffect(scavengeCancelled));
        scavengeProceeds.SetEffect(() => ScavengeProceedsRewardEffect(scavengeProceeds));
        normalScavenge.SetEffect(() => NormalScavengeRewardEffect(normalScavenge));

        _states.Add(startState.name, startState);
        _states.Add(scavengeCancelled.name, scavengeCancelled);
        _states.Add(scavengeProceeds.name, scavengeProceeds);
        _states.Add(normalScavenge.name, normalScavenge);
        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stopThem = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 50, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PursuadeToCancelEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingEffect(state),
            };
            state.AddActionOption(stopThem);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override void EndInteraction() {
        base.EndInteraction();
        //TODO: _characterInvolved.SetForce
    }
    #endregion

    #region Action Option Effects
    private void PursuadeToCancelEffect(InteractionState state) {
        //Compute Dissuader success rate
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);
        AddToDebugLog("Chose to pursuade to cancel. " + resultWeights.GetWeightsSummary("Summary of weights are: "));
        string nextState = string.Empty;
        RESULT result = resultWeights.PickRandomElementGivenWeights();
        AddToDebugLog("Result of weights is " + result);
        switch (result) {
            case RESULT.SUCCESS:
                nextState = Scavenge_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Scavenge_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        AddToDebugLog("Chose to do nothing");
        SetCurrentState(_states[Normal_Scavenge]);
    }
    #endregion

    private void ScavengeCancelledRewardEffect(InteractionState state) {
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void ScavengeProceedsRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Scavenge Event.
        StartMove();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void NormalScavengeRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Scavenge Event.
        StartMove();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }

    private void StartMove() {
        AddToDebugLog(_characterInvolved.name + " starts moving towards " + targetArea.name + "(" + targetArea.coreTile.landmarkOnTile.name + ")");
        _characterInvolved.ownParty.GoToLocation(targetArea, PATHFINDING_MODE.NORMAL, null, () => CreateScavengeEvent());
    }
    private void CreateScavengeEvent() {
        AddToDebugLog(_characterInvolved.name + " will now create scavenge event");
        Interaction scavenge = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SCAVENGE_EVENT, _characterInvolved.specificLocation);
        scavenge.SetCanInteractionBeDoneAction(IsScavengeStillValid);
        _characterInvolved.SetForcedInteraction(scavenge);
    }

    private bool IsScavengeStillValid() {
        return _characterInvolved.specificLocation != null && _characterInvolved.specificLocation.owner == null;
    }

    private Area GetTargetArea() {
        List<Area> choices = new List<Area>();
        //Select another tile with no owner as the Scavenge target.
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.owner == null) {
                choices.Add(currArea);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        throw new System.Exception("Cannot find target area for move to scavenge event at " + interactable.name);
    }
}
