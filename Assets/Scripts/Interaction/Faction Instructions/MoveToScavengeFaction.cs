using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToScavengeFaction : Interaction {

    private const string Scavenge_Cancelled = "Scavenge Cancelled";
    private const string Scavenge_Proceeds = "Scavenge Proceeds";
    private const string Normal_Scavenge = "Normal Scavenge";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.SCAVENGE_EVENT_FACTION; }
    }

    public MoveToScavengeFaction(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_SCAVENGE_EVENT_FACTION, 0) {
        _name = "Move To Scavenge Faction";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState scavengeCancelled = new InteractionState(Scavenge_Cancelled, this);
        InteractionState scavengeProceeds = new InteractionState(Scavenge_Proceeds, this);
        InteractionState normalScavenge = new InteractionState(Normal_Scavenge, this);

        _targetArea = GetTargetArea();
        AddToDebugLog("Set target area to " + _targetArea.name);
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        scavengeCancelled.SetEffect(() => ScavengeCancelledRewardEffect(scavengeCancelled));
        scavengeProceeds.SetEffect(() => ScavengeProceedsRewardEffect(scavengeProceeds));
        normalScavenge.SetEffect(() => NormalScavengeRewardEffect(normalScavenge));

        _states.Add(startState.name, startState);
        _states.Add(scavengeCancelled.name, scavengeCancelled);
        _states.Add(scavengeProceeds.name, scavengeProceeds);
        _states.Add(normalScavenge.name, normalScavenge);
        //SetCurrentState(startState);
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
    public override void DoActionUponMoveToArrival() {
        AddToDebugLog(_characterInvolved.name + " will now create scavenge event");
        CreateConnectedEvent(INTERACTION_TYPE.SCAVENGE_EVENT_FACTION, _characterInvolved.specificLocation);
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
        //investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void ScavengeProceedsRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Scavenge Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void NormalScavengeRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Scavenge Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }

    //private void CreateEvent() {
    //    AddToDebugLog(_characterInvolved.name + " will now create scavenge event");
    //    Interaction scavenge = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.SCAVENGE_EVENT_FACTION, _characterInvolved.specificLocation);
    //    _characterInvolved.SetForcedInteraction(scavenge);
    //}

    private Area GetTargetArea() {
        List<Area> choices = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id == PlayerManager.Instance.player.playerArea.id || currArea.owner != null) {
                continue;
            }
            if (currArea.HasStructure(STRUCTURE_TYPE.DUNGEON)
                || currArea.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
                choices.Add( currArea);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
