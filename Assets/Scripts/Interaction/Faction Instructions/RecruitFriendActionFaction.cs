using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecruitFriendActionFaction : Interaction {

    private Character _targetCharacter;

    private const string Normal_Recruitment_Success = "Normal Recruitment Success";
    private const string Normal_Recruitment_Fail = "Normal Recruitment Fail";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public RecruitFriendActionFaction(Area interactable) 
        : base(interactable, INTERACTION_TYPE.RECRUIT_FRIEND_ACTION_FACTION, 0) {
        _name = "Recruit Friend Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalRecruitmenSuccess = new InteractionState(Normal_Recruitment_Success, this);
        InteractionState normalRecruitmentFail = new InteractionState(Normal_Recruitment_Fail, this);

        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        normalRecruitmenSuccess.SetEffect(() => NormalRecruitmentSuccessRewardEffect(normalRecruitmenSuccess));
        normalRecruitmentFail.SetEffect(() => NormalRecruitmentFailRewardEffect(normalRecruitmentFail));

        _states.Add(startState.name, startState);
        _states.Add(normalRecruitmenSuccess.name, normalRecruitmenSuccess);
        _states.Add(normalRecruitmentFail.name, normalRecruitmentFail);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (interactable.IsResidentsFull()) {
            return false;
        }
        if (GetTargetCharacter(character) == null) { //check if a target character can be found using the provided weights
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
        AddToDebugLog("Set target character to " + targetCharacter.name);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Recruitment_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Recruitment_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void NormalRecruitmentSuccessRewardEffect(InteractionState state) {
        /* Mechanics**: Transfer Character 2 to Character 1's Faction. 
         * Change its home to be the same as Character 1's home Area. 
         * Override his next action as https://trello.com/c/PTkSE6DZ/439-character-move-to-return-home
         */
        TransferCharacter(_targetCharacter, _characterInvolved.faction);
        //**Level Up**: Recruiting Character +1
        //_characterInvolved.LevelUp();

        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
    }
    private void NormalRecruitmentFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion

    private void TransferCharacter(Character character, Faction faction) {
        character.faction.RemoveCharacter(character);
        faction.AddNewCharacter(character);
        character.MigrateHomeTo(_characterInvolved.homeArea);
        Interaction interaction = InteractionManager.Instance.CreateNewInteraction(INTERACTION_TYPE.MOVE_TO_RETURN_HOME, character.specificLocation);
        character.SetForcedInteraction(interaction);
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (characterInvolved.GetFriendTraitWith(currCharacter) != null //- personal friend of the character
                && (currCharacter.isFactionless || currCharacter.faction.id != characterInvolved.faction.id)  //- unaligned or from a different faction
                && currCharacter.role.roleType != CHARACTER_ROLE.BEAST //- not a Beast and not a Skeleton
                && currCharacter.race != RACE.SKELETON
                && !currCharacter.HasTraitOf(TRAIT_TYPE.DISABLER)) { //- must not have a Disabler trait
                choices.Add(currCharacter);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
