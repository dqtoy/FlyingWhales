using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatAbducted : Interaction {

    private Character _targetCharacter;

    private const string Start = "Start";
    private const string Release_Success = "Release Success";
    private const string Release_Fail = "Release Fail";
    private const string Release_Critical_Fail = "Release Critical Fail";
    private const string Persuade_Success = "Persuade Success";
    private const string Persuade_Fail = "Persuade Fail";
    private const string Persuade_Critical_Fail = "Persuade Critical Fail";
    private const string Character_Eaten = "Character Eaten";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public EatAbducted(BaseLandmark interactable): base(interactable, INTERACTION_TYPE.EAT_ABDUCTED, 0) {
        _name = "Eat Abducted";
        _jobFilter = new JOB[] { JOB.DIPLOMAT, JOB.DEBILITATOR };
    }

    #region Override
    public override void CreateStates() {
        if (_targetCharacter == null) {
            SetTargetCharacter(GetTargetCharacter(_characterInvolved));
        }

        InteractionState startState = new InteractionState(Start, this);
        InteractionState releaseSuccess = new InteractionState(Release_Success, this);
        InteractionState releaseFail = new InteractionState(Release_Fail, this);
        InteractionState releaseCriticalFail = new InteractionState(Release_Critical_Fail, this);
        InteractionState persuadeSuccess = new InteractionState(Persuade_Success, this);
        InteractionState persuadeFail = new InteractionState(Persuade_Fail, this);
        InteractionState persuadeCriticalFail = new InteractionState(Persuade_Critical_Fail, this);
        InteractionState characterEaten = new InteractionState(Character_Eaten, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        releaseSuccess.SetEffect(() => ReleaseSuccessEffect(releaseSuccess));
        releaseFail.SetEffect(() => ReleaseFailEffect(releaseFail));
        releaseCriticalFail.SetEffect(() => ReleaseCritFailEffect(releaseCriticalFail));

        persuadeSuccess.SetEffect(() => PersuadeSuccessEffect(persuadeSuccess));
        persuadeFail.SetEffect(() => PersuadeFailEffect(persuadeFail));
        persuadeCriticalFail.SetEffect(() => PersuadeCritFailEffect(persuadeCriticalFail));

        characterEaten.SetEffect(() => CharacterEatenEffect(characterEaten));

        _states.Add(startState.name, startState);
        _states.Add(persuadeSuccess.name, persuadeSuccess);
        _states.Add(persuadeFail.name, persuadeFail);
        _states.Add(persuadeCriticalFail.name, persuadeCriticalFail);

        _states.Add(releaseSuccess.name, releaseSuccess);
        _states.Add(releaseFail.name, releaseFail);
        _states.Add(releaseCriticalFail.name, releaseCriticalFail);

        _states.Add(characterEaten.name, characterEaten);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption release = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Release " + _targetCharacter.name + " before " + Utilities.GetPronounString(_targetCharacter.gender, PRONOUN_TYPE.SUBJECTIVE, false) + " gets eaten.",
                effect = () => ReleaseOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                disabledTooltipText = "Minion must be a Diplomat",
            };
            ActionOption persuade = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Persuade to stop " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.POSSESSIVE, false) + " plan to eat " + _targetCharacter.name + ".",
                effect = () => PersuadeOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                disabledTooltipText = "Minion must be a Dissuader",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(release);
            state.AddActionOption(persuade);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        SetTargetCharacter(GetTargetCharacter(character));
        if (_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void ReleaseOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
            nextState = Release_Success;
            break;
            case RESULT.FAIL:
            nextState = Release_Fail;
            break;
            case RESULT.CRITICAL_FAIL:
            nextState = Release_Critical_Fail;
            break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void PersuadeOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
            nextState = Persuade_Success;
            break;
            case RESULT.FAIL:
            nextState = Persuade_Fail;
            break;
            case RESULT.CRITICAL_FAIL:
            nextState = Persuade_Critical_Fail;
            break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Character_Eaten]);
    }
    #endregion

    #region Reward Effect
    private void ReleaseSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        investigatorCharacter.LevelUp();
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _targetCharacter.faction, 1, state);

        _targetCharacter.ReleaseFromAbduction();
    }
    private void ReleaseFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();
        _targetCharacter.Death();
    }
    private void ReleaseCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));

        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _characterInvolved.faction, -1, state);

        _characterInvolved.LevelUp();
        _targetCharacter.Death();
        investigatorCharacter.Death();
    }
    private void PersuadeSuccessEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        investigatorCharacter.LevelUp();
    }
    private void PersuadeFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _characterInvolved.LevelUp();
        _targetCharacter.Death();
    }
    private void PersuadeCritFailEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        state.descriptionLog.AddToFillers(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(investigatorCharacter, investigatorCharacter.name, LOG_IDENTIFIER.MINION_1));

        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _characterInvolved.faction, -1, state);

        _characterInvolved.LevelUp();
        _targetCharacter.Death();
        investigatorCharacter.Death();
    }
    private void CharacterEatenEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);

        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        _targetCharacter.Death();
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        WeightedDictionary<Character> characterWeights = new WeightedDictionary<Character>();
        for (int i = 0; i < interactable.tileLocation.areaOfTile.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.tileLocation.areaOfTile.charactersAtLocation[i];
            if (currCharacter.id != characterInvolved.id && !currCharacter.currentParty.icon.isTravelling && currCharacter.IsInOwnParty() && currCharacter.GetTrait("Abducted") != null) {
                return currCharacter;
            }
        }
        return null;
        //throw new System.Exception("Could not find any character to recruit!");
    }
}
