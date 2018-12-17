using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToExplore : Interaction {

    private const string Character_Explore_Cancelled = "Character Explore Cancelled";
    private const string Character_Explore_Continues = "Character Explore Continues";
    private const string Do_Nothing = "Do nothing";

    private Area targetLocation;

    public MoveToExplore(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_EXPLORE, 0) {
        _name = "Move to Explore";
        _jobFilter = new JOB[] { JOB.DISSUADER };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterExploreCancelled = new InteractionState(Character_Explore_Cancelled, this);
        InteractionState characterExploreContinues = new InteractionState(Character_Explore_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        targetLocation = GetTargetLocation();

        //**Text Description**: [Character Name] is about to leave for [Location Name 1] to scavenge for supplies.
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
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) +" from leaving.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
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
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    #endregion

    #region Option Effects
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorMinion.character.job.GetJobRateWeights();
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
        investigatorMinion.LevelUp();
    }
    private void CharacterExploreContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Character will start its travel to selected location to start an Explore event.
        GoToTargetLocation();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //**Mechanics**: Character will start its travel to selected location to start an Explore event.
        GoToTargetLocation();
        state.AddLogFiller(new LogFiller(targetLocation, targetLocation.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    private void GoToTargetLocation() {
        _characterInvolved.ownParty.GoToLocation(targetLocation.coreTile.landmarkOnTile, PATHFINDING_MODE.NORMAL, () => CreateExploreEvent());
    }

    private void CreateExploreEvent() {
        if (_characterInvolved.job is Explorer) {
            Interaction exploreEvent = (_characterInvolved.job as Explorer).CreateExplorerEvent();
            if (exploreEvent != null) {
                _characterInvolved.SetForcedInteraction(exploreEvent);
            }
        } 
        //else if (_characterInvolved.job is Spy) {
        //    Interaction exploreEvent = (_characterInvolved.job as Spy).CreateExplorerEvent();
        //    _characterInvolved.SetForcedInteraction(exploreEvent);
        //}
    }

    private Area GetTargetLocation() {
        List<Area> choices = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (_characterInvolved.specificLocation.tileLocation.areaOfTile.id != currArea.id 
                && PlayerManager.Instance.player.playerArea.id != currArea.id) {
                //&& !currArea.IsHostileTowards(_characterInvolved)
                choices.Add(currArea);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
