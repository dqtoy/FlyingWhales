using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToOccupyFaction : Interaction {

    private const string Character_Expand_Cancelled = "Character Expand Cancelled";
    private const string Character_Expand_Continues = "Character Expand Continues";
    private const string Character_Normal_Expand = "Character Normal Expand";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    private Faction targetFaction;

    public MoveToOccupyFaction(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_OCCUPY_FACTION, 0) {
        _name = "Move To Occupy";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetTargetArea(Area target) {
        _targetArea = target;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState expandCancelled = new InteractionState(Character_Expand_Cancelled, this);
        InteractionState expandContinues = new InteractionState(Character_Expand_Continues, this);
        InteractionState normalExpand = new InteractionState(Character_Normal_Expand, this);

        _targetArea = GetTargetArea(_characterInvolved);
        targetFaction = _targetArea.owner;
        AddToDebugLog("Set target area to " + _targetArea.name);
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        expandCancelled.SetEffect(() => ExpandCancelledRewardEffect(expandCancelled));
        expandContinues.SetEffect(() => ExpandContinuesRewardEffect(expandContinues));
        normalExpand.SetEffect(() => NormalExpandRewardEffect(normalExpand));

        _states.Add(startState.name, startState);
        _states.Add(expandCancelled.name, expandCancelled);
        _states.Add(expandContinues.name, expandContinues);
        _states.Add(normalExpand.name, normalExpand);
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
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetArea(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void DoActionUponMoveToArrival() {
        CreateConnectedEvent(INTERACTION_TYPE.OCCUPY_ACTION_FACTION, _characterInvolved.specificLocation);
    }
    #endregion

    #region Action Option Effects
    private void PursuadeToCancelEffect(InteractionState state) {
        //Compute Dissuader success rate
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        AddToDebugLog("Chose to pursuade to cancel. " + resultWeights.GetWeightsSummary("Summary of weights are: "));
        string nextState = string.Empty;
        RESULT result = resultWeights.PickRandomElementGivenWeights();
        AddToDebugLog("Result of weights is " + result);
        switch (result) {
            case RESULT.SUCCESS:
                nextState = Character_Expand_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Expand_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        AddToDebugLog("Chose to do nothing");
        SetCurrentState(_states[Character_Normal_Expand]);
    }
    #endregion

    private void ExpandCancelledRewardEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void ExpandContinuesRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void NormalExpandRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }

    //private void CreateEvent() {
    //    Interaction newEvent = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.OCCUPY_ACTION_FACTION, _characterInvolved.specificLocation);
    //    _characterInvolved.SetForcedInteraction(newEvent);
    //}

    private Area GetTargetArea(Character character) {
        List<Area> choices = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area area = LandmarkManager.Instance.allAreas[i];
            if (area.id != character.specificLocation.id
                && area.id != PlayerManager.Instance.player.playerArea.id
                && area.owner == null
                && area.possibleOccupants.Contains(character.race)) {
                choices.Add(area);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
