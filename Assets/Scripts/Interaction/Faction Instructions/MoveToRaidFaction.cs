using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToRaidFaction : Interaction {

    private const string Raid_Cancelled = "Raid Cancelled";
    private const string Raid_Proceeds = "Raid Proceeds";
    private const string Normal_Raid = "Normal Raid";

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    private Faction targetFaction;

    public MoveToRaidFaction(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_RAID_FACTION, 0) {
        _name = "Move To Raid";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetTargetArea(Area target) {
        _targetArea = target;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState raidCancelled = new InteractionState(Raid_Cancelled, this);
        InteractionState raidProceeds = new InteractionState(Raid_Proceeds, this);
        InteractionState normalRaid = new InteractionState(Normal_Raid, this);

        _targetArea = GetTargetArea(_characterInvolved);
        targetFaction = _targetArea.owner;
        AddToDebugLog("Set target area to " + _targetArea.name);
        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        raidCancelled.SetEffect(() => RaidCancelledRewardEffect(raidCancelled));
        raidProceeds.SetEffect(() => RaidProceedsRewardEffect(raidProceeds));
        normalRaid.SetEffect(() => NormalRaidRewardEffect(normalRaid));

        _states.Add(startState.name, startState);
        _states.Add(raidCancelled.name, raidCancelled);
        _states.Add(raidProceeds.name, raidProceeds);
        _states.Add(normalRaid.name, normalRaid);
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
        AddToDebugLog(_characterInvolved.name + " will now create raid event");
        CreateConnectedEvent(INTERACTION_TYPE.RAID_EVENT_FACTION, _characterInvolved.specificLocation);
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
                nextState = Raid_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Raid_Proceeds;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        AddToDebugLog("Chose to do nothing");
        SetCurrentState(_states[Normal_Raid]);
    }
    #endregion

    private void RaidCancelledRewardEffect(InteractionState state) {
        investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void RaidProceedsRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void NormalRaidRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }

    //private void CreateEvent() {
        
    //    Interaction raid = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_EVENT_FACTION, _characterInvolved.specificLocation);
    //    _characterInvolved.SetForcedInteraction(raid);
    //}

    private Area GetTargetArea(Character character) {
        List<Area> choices = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id == PlayerManager.Instance.player.playerArea.id 
                || currArea.owner == null 
                || !currArea.HasStructure(STRUCTURE_TYPE.WAREHOUSE)) {
                continue;
            }
            if (currArea.owner.id != character.faction.id) {
                switch (currArea.owner.GetRelationshipWith(character.faction).relationshipStatus) {
                    case FACTION_RELATIONSHIP_STATUS.AT_WAR:
                    case FACTION_RELATIONSHIP_STATUS.ENEMY:
                    case FACTION_RELATIONSHIP_STATUS.DISLIKED:
                    case FACTION_RELATIONSHIP_STATUS.NEUTRAL:
                        choices.Add(currArea);
                        break;
                }
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
