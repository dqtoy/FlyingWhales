﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToExploreFaction : Interaction {

    private const string Character_Explore_Cancelled = "Character Explore Cancelled";
    private const string Character_Explore_Continues = "Character Explore Continues";
    private const string Do_Nothing = "Do nothing";

    private Area targetLocation;

    public MoveToExploreFaction(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_EXPLORE, 0) {
        _name = "Move to Explore Faction";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
        _categories = new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.INVENTORY };
        _alignment = INTERACTION_ALIGNMENT.NEUTRAL;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterExploreCancelled = new InteractionState(Character_Explore_Cancelled, this);
        InteractionState characterExploreContinues = new InteractionState(Character_Explore_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        targetLocation = GetTargetLocation(_characterInvolved);
        AddToDebugLog(_characterInvolved.name + " chose to explore " + targetLocation.name);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(_characterInvolved.race), LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterExploreCancelled.SetEffect(() => CharacterExploreCancelledRewardEffect(characterExploreCancelled));
        characterExploreContinues.SetEffect(() => CharacterExploreContinuesRewardEffect(characterExploreContinues));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(characterExploreCancelled.name, characterExploreCancelled);
        _states.Add(characterExploreContinues.name, characterExploreContinues);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) +" from leaving.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
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
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (GetTargetLocation(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Character_Explore_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Explore_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void CharacterExploreCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
    }
    private void CharacterExploreContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Character will start its travel to selected location to start an Explore event.
        GoToTargetLocation();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Character will start its travel to selected location to start an Explore event.
        GoToTargetLocation();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    private void GoToTargetLocation() {
        AddToDebugLog(_characterInvolved.name + " will  no go to " + targetLocation.name + " to explore");
        _characterInvolved.ownParty.GoToLocation(targetLocation, PATHFINDING_MODE.NORMAL, null, () => CreateExploreEvent());
    }

    private void CreateExploreEvent() {
        Interaction exploreEvent = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.EXPLORE_EVENT_FACTION, _characterInvolved.specificLocation);
        if (exploreEvent != null) {
            _characterInvolved.SetForcedInteraction(exploreEvent);
        }
    }

    private Area GetTargetLocation(Character character) {
        /*
         For all Locations with a Dungeon, randomly select based on the following weights: 
         */
        WeightedDictionary<Area> choices = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id != PlayerManager.Instance.player.playerArea.id && currArea.HasStructure(STRUCTURE_TYPE.DUNGEON)) {
                int weight = 10; //Base Weight +10
                if (currArea.owner == null) {
                    weight += 10; //Location is unoccupied: Weight +10
                }
                choices.AddElement(currArea, weight);
            }
        }
        if (choices.GetTotalOfWeights() > 0) {
            return choices.PickRandomElementGivenWeights();
        }
        return null;
    }
}