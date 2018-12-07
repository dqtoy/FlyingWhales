﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToRaid : Interaction {

    private const string Raid_Cancelled = "Raid Cancelled";
    private const string Raid_Proceeds = "Raid Proceeds";
    private const string Normal_Raid = "Normal Raid";

    private Area targetArea;

    public MoveToRaid(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_RAID, 0) {
        _name = "Move To Raid";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState raidCancelled = new InteractionState(Raid_Cancelled, this);
        InteractionState raidProceeds = new InteractionState(Raid_Proceeds, this);
        InteractionState normalRaid = new InteractionState(Normal_Raid, this);

        targetArea = GetTargetArea();
        AddToDebugLog("Set target area to " + targetArea.name);
        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
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
                name = "Pursuade " + _characterInvolved.name + " to cancel " + Utilities.GetPossessivePronounForCharacter(_characterInvolved, false) + " plans.",
                duration = 0,
                effect = () => PursuadeToCancelEffect(state),
                jobNeeded = JOB.DISSUADER,
                doesNotMeetRequirementsStr = "Minion must be a dissuader",
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
        SetCurrentState(_states[Raid_Proceeds]);
    }
    #endregion

    private void RaidCancelledRewardEffect(InteractionState state) {
        //**Text Description**: [Demon Name] convinced [Character Name] to cancel [his/her] plan to raid [Location Name].
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        //**Log**: [Demon Name] persuaded [Character Name] to stop [his/her] plans to raid [Location Name].
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
        explorerMinion.LevelUp();
    }
    private void RaidProceedsRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMove();
        //**Text Description**: [Demon Name] failed to convince [Character Name] to cancel [his/her] plan to raid [Location Name].
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        //**Log**: [Demon Name] failed to persuade [Character Name] to stop [his/her] plans to raid [Location Name].
        //**Log**: [Character Name] left to raid [Location Name].
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }
    private void NormalRaidRewardEffect(InteractionState state) {
        //Selected character will travel to Location 1 to start a Raid Event.
        StartMove();
        //**Text Description**: [Demon Name] did not interfere with [Character Name]'s plans.
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        }
        //**Log**: [Character Name] left to raid [Location Name].
        state.AddLogFiller(new LogFiller(targetArea, targetArea.name, LOG_IDENTIFIER.LANDMARK_1));
    }

    private void StartMove() {
        AddToDebugLog(_characterInvolved.name + " starts moving towards " + targetArea.name + "(" + targetArea.coreTile.landmarkOnTile.name + ")");
        _characterInvolved.ownParty.GoToLocation(targetArea.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateRaidEvent());
    }
    private void CreateRaidEvent() {
        AddToDebugLog(_characterInvolved.name + " will now create raid event");
        Interaction raid = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RAID_EVENT, _characterInvolved.specificLocation.tileLocation.landmarkOnTile);
        _characterInvolved.SetForcedInteraction(raid);
    }

    private Area GetTargetArea() {
        List<Area> choices = new List<Area>();
        //Select another area owned by a different Faction as the Raid target.
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.owner != null && currArea.id != PlayerManager.Instance.player.playerArea.id 
                && currArea.owner.id != _characterInvolved.faction.id) {
                choices.Add(currArea);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        throw new System.Exception("Cannot find target area for move to scavenge event at " + interactable.name);
    }
}