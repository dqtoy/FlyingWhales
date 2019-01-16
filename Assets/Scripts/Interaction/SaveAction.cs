using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveAction : Interaction {

    private Character _targetCharacter;

    private const string Assisted_Release_Success = "Assisted Release Success";
    private const string Assisted_Release_Fail = "Assisted Release Fail";
    private const string Assisted_Release_Critical_Fail = "Assisted Release Critical Fail";

    private const string Thwarted_Release_Success = "Thwarted Release Success";
    private const string Thwarted_Release_Fail = "Thwarted Release Fail";
    private const string Thwarted_Release_Critical_Fail = "Thwarted Release Critical Fail";
    
    private const string Normal_Release_Success = "Normal Release Success";
    private const string Normal_Release_Fail = "Normal Release Fail";
    private const string Normal_Release_Critical_Fail = "Normal Release Critical Fail";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public SaveAction(BaseLandmark interactable) 
        : base(interactable, INTERACTION_TYPE.SAVE_ACTION, 0) {
        _name = "Save Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState thwartedReleaseSuccess = new InteractionState(Thwarted_Release_Success, this);
        InteractionState thwartedReleaseFail = new InteractionState(Thwarted_Release_Fail, this);
        InteractionState thwartedReleaseCriticalFail = new InteractionState(Thwarted_Release_Critical_Fail, this);
        InteractionState assistedReleaseSuccess = new InteractionState(Assisted_Release_Success, this);
        InteractionState assistedReleaseFail = new InteractionState(Assisted_Release_Fail, this);
        InteractionState assistedReleaseCriticalFail = new InteractionState(Assisted_Release_Critical_Fail, this);
        InteractionState normalReleaseSuccess = new InteractionState(Normal_Release_Success, this);
        InteractionState normalReleaseFail = new InteractionState(Normal_Release_Fail, this);
        InteractionState normalReleaseCriticalFail = new InteractionState(Normal_Release_Critical_Fail, this);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        thwartedReleaseSuccess.SetEffect(() => ThwartedReleaseSuccessRewardEffect(thwartedReleaseSuccess));
        thwartedReleaseFail.SetEffect(() => ThwartedReleaseFailRewardEffect(thwartedReleaseFail));
        thwartedReleaseCriticalFail.SetEffect(() => ThwartedReleaseCriticalFailRewardEffect(thwartedReleaseCriticalFail));

        assistedReleaseSuccess.SetEffect(() => AssistedReleaseSuccessRewardEffect(assistedReleaseSuccess));
        assistedReleaseFail.SetEffect(() => AssistedReleaseFailRewardEffect(assistedReleaseFail));
        assistedReleaseCriticalFail.SetEffect(() => AssistedReleaseCriticalFailRewardEffect(assistedReleaseCriticalFail));

        normalReleaseSuccess.SetEffect(() => NormalReleaseSuccessRewardEffect(normalReleaseSuccess));
        normalReleaseFail.SetEffect(() => NormalReleaseFailRewardEffect(normalReleaseFail));
        normalReleaseCriticalFail.SetEffect(() => NormalReleaseCriticalFailRewardEffect(normalReleaseCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(thwartedReleaseSuccess.name, thwartedReleaseSuccess);
        _states.Add(thwartedReleaseFail.name, thwartedReleaseFail);
        _states.Add(thwartedReleaseCriticalFail.name, thwartedReleaseCriticalFail);

        _states.Add(assistedReleaseSuccess.name, assistedReleaseSuccess);
        _states.Add(assistedReleaseFail.name, assistedReleaseFail);
        _states.Add(assistedReleaseCriticalFail.name, assistedReleaseCriticalFail);

        _states.Add(normalReleaseSuccess.name, normalReleaseSuccess);
        _states.Add(normalReleaseFail.name, normalReleaseFail);
        _states.Add(normalReleaseCriticalFail.name, normalReleaseCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist " + _characterInvolved.name,
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption thwart = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Thwart " + _characterInvolved.name,
                effect = () => ThwartOptionEffect(state),
                jobNeeded = JOB.DIPLOMAT,
                doesNotMeetRequirementsStr = "Must have diplomat minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(assist);
            state.AddActionOption(thwart);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        /*
         Once the actual action is triggered, the character will check if the target to be saved is still in the location and 
         if its original home still has available resident capacity.
         */
        if (_targetCharacter == null 
            || _targetCharacter.specificLocation.tileLocation.areaOfTile.id != interactable.tileLocation.areaOfTile.id
            || _targetCharacter.GetTrait("Abducted") == null
            || (_targetCharacter.GetTrait("Abducted") as Abducted).originalHomeLandmark.tileLocation.areaOfTile.IsResidentsFull()) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void AssistOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.SUCCESS, 30);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Assisted_Release_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Release_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Release_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void ThwartOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Thwarted_Release_Success;
                break;
            case RESULT.FAIL:
                nextState = Thwarted_Release_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Thwarted_Release_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Normal_Release_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Release_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Release_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedReleaseSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Remove Abducted trait from Character 2. Change Character 2 Home to its original one. Override his next tick to return home.
        _targetCharacter.ReleaseFromAbduction();

        //**Mechanics**: Abducted and Releaser personal relationship +1
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, 1);

        //**Mechanics**: Relationship between the two factions -1
        AdjustFactionsRelationship(_characterInvolved.faction, interactable.tileLocation.areaOfTile.owner, -1, state);

        //**Level Up**: Releaser Character +1, Instigator Minion +1
        _characterInvolved.LevelUp();
        investigatorCharacter.LevelUp();
    }
    private void AssistedReleaseFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Relationship between the two factions -1
        AdjustFactionsRelationship(_characterInvolved.faction, interactable.tileLocation.areaOfTile.owner, -1, state);
    }
    private void AssistedReleaseCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void ThwartedReleaseSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Remove Abducted trait from Character 2. Change Character 2 Home to its original one. Override his next tick to return home.
        _targetCharacter.ReleaseFromAbduction();

        //**Mechanics**: Abducted and Releaser personal relationship +1
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, 1);

        //**Level Up**: Releaseer Character +1
        _characterInvolved.LevelUp();
    }
    private void ThwartedReleaseFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_2));

        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
        //**Mechanics**: Relationship between the player faction and Faction 2 +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, interactable.tileLocation.areaOfTile.owner, 1, state);
    }
    private void ThwartedReleaseCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(interactable.tileLocation.areaOfTile.owner, interactable.tileLocation.areaOfTile.owner.name, LOG_IDENTIFIER.FACTION_2));

        //**Mechanics**: Relationship between the player faction and Faction 2 +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, interactable.tileLocation.areaOfTile.owner, 1, state);
        
        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();

        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
    }
    private void NormalReleaseSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Remove Abducted trait from Character 2. Change Character 2 Home to its original one. Override his next tick to return home.
        _targetCharacter.ReleaseFromAbduction();

        //**Mechanics**: Abducted and Releaser personal relationship +1
        CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, 1);

        //**Level Up**: Releaseer Character +1
        _characterInvolved.LevelUp();
    }
    private void NormalReleaseFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalReleaseCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    #endregion

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
        AddToDebugLog("Set " + targetCharacter.name + " as target");
    }
}
