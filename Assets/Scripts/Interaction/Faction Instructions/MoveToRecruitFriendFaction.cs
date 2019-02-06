using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToRecruitFriendFaction : Interaction {

    private Area _targetArea;

    public override Area targetArea {
        get { return _targetArea; }
    }
    public override INTERACTION_TYPE pairedInteractionType {
        get { return INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION; }
    }

    private const string Character_Recruit_Cancelled = "Character Recruit Cancelled";
    private const string Character_Recruit_Continues = "Character Recruit Continues";
    private const string Do_Nothing = "Do nothing";

    public MoveToRecruitFriendFaction(Area interactable) : base(interactable, INTERACTION_TYPE.MOVE_TO_RECRUIT_FRIEND_ACTION_FACTION, 0) {
        _name = "Move To Recruit Friend";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterRecruitCancelled = new InteractionState(Character_Recruit_Cancelled, this);
        InteractionState characterRecruitContinues = new InteractionState(Character_Recruit_Continues, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        _targetArea = GetTargetLocation(_characterInvolved);
        
        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        characterRecruitCancelled.SetEffect(() => CharacterRecruitCancelledRewardEffect(characterRecruitCancelled));
        characterRecruitContinues.SetEffect(() => CharacterRecruitContinuesRewardEffect(characterRecruitContinues));
        doNothing.SetEffect(() => DoNothingExpandRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(characterRecruitCancelled.name, characterRecruitCancelled);
        _states.Add(characterRecruitContinues.name, characterRecruitContinues);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from leaving.",
                duration = 0,
                effect = () => PreventOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                duration = 0,
                effect = () => DoNothingOptionEffect(state),
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
    public override void DoActionUponMoveToArrival() {
        CreateConnectedEvent(INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION, _targetArea);
    }
    #endregion

    #region Action Option Effects
    private void PreventOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Character_Recruit_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Character_Recruit_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effects
    private void CharacterRecruitCancelledRewardEffect(InteractionState state) {
        //**Mechanics**: Character will no longer leave.
        //**Level Up**: Dissuader Minion +1
        //investigatorCharacter.LevelUp();
        MinionSuccess();
    }
    private void CharacterRecruitContinuesRewardEffect(InteractionState state) {
        StartMoveToAction();
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2);
        }
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    private void DoNothingExpandRewardEffect(InteractionState state) {
        StartMoveToAction();
        state.AddLogFiller(new LogFiller(_targetArea, _targetArea.name, LOG_IDENTIFIER.LANDMARK_2));
    }
    #endregion

    //private void CreateEvent() {
    //    Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION, _targetArea);
    //    _characterInvolved.SetForcedInteraction(interaction);
    //}

    private Area GetTargetLocation(Character characterInvolved) {
        /*
         Location Selection Weights:
        - location is Home of a personal Friend that is not from the character's faction and is not a Beast nor a Skeleton: Weight +15
         */
        WeightedDictionary<Area> locationWeights = new WeightedDictionary<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area currArea = LandmarkManager.Instance.allAreas[i];
            if (currArea.id == PlayerManager.Instance.player.playerArea.id) {
                continue; //skip
            }
            int weight = 0;
            if (AreaHasNonFactionFriendResident(currArea, characterInvolved)) {
                weight += 15;
            }
            if (weight > 0) {
                locationWeights.AddElement(currArea, weight);
            }
        }
        if (locationWeights.GetTotalOfWeights() > 0) {
            return locationWeights.PickRandomElementGivenWeights();
        }
        return null;
        //throw new System.Exception(GameManager.Instance.TodayLogString() + _characterInvolved.name + " could not find any location to recruit at!");
    }
    private bool AreaHasNonFactionFriendResident(Area area, Character characterInvolved) {
        for (int i = 0; i < area.areaResidents.Count; i++) {
            Character character = area.areaResidents[i];
            if (!character.isFactionless 
                && character.faction.id != characterInvolved.faction.id
                && character.id != characterInvolved.id
                && !character.isLeader
                && character.GetFriendTraitWith(characterInvolved) != null
                && !Utilities.IsRaceBeast(character.race)
                && character.race != RACE.SKELETON) {
                return true;
            }
        }
        return false;
    }
}
