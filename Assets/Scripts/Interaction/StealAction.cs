using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealAction : Interaction {

    private Character _targetCharacter;

    private const string Assisted_Theft_Success = "Assisted Theft Success";
    private const string Assisted_Theft_Fail = "Assisted Theft Fail";
    private const string Assisted_Theft_Critical_Fail = "Assisted Theft Critical Fail";

    private const string Thwarted_Theft_Success = "Thwarted Theft Success";
    private const string Thwarted_Theft_Fail = "Thwarted Theft Fail";
    private const string Thwarted_Theft_Critical_Fail = "Thwarted Theft Critical Fail";
    
    private const string Normal_Theft_Success = "Normal Theft Success";
    private const string Normal_Theft_Fail = "Normal Theft Fail";
    private const string Normal_Theft_Critical_Fail = "Normal Theft Critical Fail";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public StealAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.STEAL_ACTION, 0) {
        _name = "Steal Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState thwartedTheftSuccess = new InteractionState(Thwarted_Theft_Success, this);
        InteractionState thwartedTheftFail = new InteractionState(Thwarted_Theft_Fail, this);
        InteractionState thwartedTheftCriticalFail = new InteractionState(Thwarted_Theft_Critical_Fail, this);
        InteractionState assistedTheftSuccess = new InteractionState(Assisted_Theft_Success, this);
        InteractionState assistedTheftFail = new InteractionState(Assisted_Theft_Fail, this);
        InteractionState assistedTheftCriticalFail = new InteractionState(Assisted_Theft_Critical_Fail, this);
        InteractionState normalTheftSuccess = new InteractionState(Normal_Theft_Success, this);
        InteractionState normalTheftFail = new InteractionState(Normal_Theft_Fail, this);
        InteractionState normalTheftCriticalFail = new InteractionState(Normal_Theft_Critical_Fail, this);

        SetTargetCharacter(GetTargetCharacter(_characterInvolved));

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description");
        startStateDescriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);
        thwartedTheftSuccess.SetEffect(() => ThwartedTheftSuccessRewardEffect(thwartedTheftSuccess));
        thwartedTheftFail.SetEffect(() => ThwartedTheftFailRewardEffect(thwartedTheftFail));
        thwartedTheftCriticalFail.SetEffect(() => ThwartedTheftCriticalFailRewardEffect(thwartedTheftCriticalFail));

        assistedTheftSuccess.SetEffect(() => AssistedTheftSuccessRewardEffect(assistedTheftSuccess));
        assistedTheftFail.SetEffect(() => AssistedTheftFailRewardEffect(assistedTheftFail));
        assistedTheftCriticalFail.SetEffect(() => AssistedTheftCriticalFailRewardEffect(assistedTheftCriticalFail));

        normalTheftSuccess.SetEffect(() => NormalTheftSuccessRewardEffect(normalTheftSuccess));
        normalTheftFail.SetEffect(() => NormalTheftFailRewardEffect(normalTheftFail));
        normalTheftCriticalFail.SetEffect(() => NormalTheftCriticalFailRewardEffect(normalTheftCriticalFail));

        _states.Add(startState.name, startState);
        _states.Add(thwartedTheftSuccess.name, thwartedTheftSuccess);
        _states.Add(thwartedTheftFail.name, thwartedTheftFail);
        _states.Add(thwartedTheftCriticalFail.name, thwartedTheftCriticalFail);

        _states.Add(assistedTheftSuccess.name, assistedTheftSuccess);
        _states.Add(assistedTheftFail.name, assistedTheftFail);
        _states.Add(assistedTheftCriticalFail.name, assistedTheftCriticalFail);

        _states.Add(normalTheftSuccess.name, normalTheftSuccess);
        _states.Add(normalTheftFail.name, normalTheftFail);
        _states.Add(normalTheftCriticalFail.name, normalTheftCriticalFail);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption assist = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Assist with the theft.",
                effect = () => AssistOptionEffect(state),
                jobNeeded = JOB.INSTIGATOR,
                doesNotMeetRequirementsStr = "Must have instigator minion.",
            };
            ActionOption thwart = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Thwart the theft.",
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
        if (GetTargetCharacter(character) == null) {
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
        RESULT result = resultWeights.PickRandomElementGivenWeights();
        switch (result) {
            case RESULT.SUCCESS:
                nextState = Assisted_Theft_Success;
                break;
            case RESULT.FAIL:
                nextState = Assisted_Theft_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Assisted_Theft_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void ThwartOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();
        resultWeights.AddWeightToElement(RESULT.FAIL, 30);
        resultWeights.AddWeightToElement(RESULT.CRITICAL_FAIL, 20);

        string nextState = string.Empty;
        RESULT result = resultWeights.PickRandomElementGivenWeights();
        switch (result) {
            case RESULT.SUCCESS:
                nextState = Thwarted_Theft_Success;
                break;
            case RESULT.FAIL:
                nextState = Thwarted_Theft_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Thwarted_Theft_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = _characterInvolved.job.GetJobRateWeights();

        string nextState = string.Empty;
        RESULT result = resultWeights.PickRandomElementGivenWeights();
        switch (result) {
            case RESULT.SUCCESS:
                nextState = Normal_Theft_Success;
                break;
            case RESULT.FAIL:
                nextState = Normal_Theft_Fail;
                break;
            case RESULT.CRITICAL_FAIL:
                nextState = Normal_Theft_Critical_Fail;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void AssistedTheftSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new  LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));

        //**Mechanics**: Transfer item from target character to the thief.
        TransferItem(_targetCharacter, _characterInvolved);

        if (_targetCharacter.faction.id != _characterInvolved.faction.id) {
            //**Mechanics**: If different faction, relationship between the two factions -1.
            AdjustFactionsRelationship(_targetCharacter.faction, _characterInvolved.faction, -1, state);
        } else {
            //**Mechanics**: If same faction, personal relationship between the two characters -1. Thief gains Criminal trait.
            CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, -1);
            _characterInvolved.AddTrait(new Criminal());
        }


        //**Level Up**: Thief Character +1, Instigator +1
        _characterInvolved.LevelUp();
        investigatorCharacter.LevelUp();
    }
    private void AssistedTheftFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_1));


        if (_targetCharacter.faction.id != _characterInvolved.faction.id) {
            //**Mechanics**: If different faction, relationship between the two factions -1.
            AdjustFactionsRelationship(_targetCharacter.faction, _characterInvolved.faction, -1, state);
        } else {
            //**Mechanics**: If same faction, personal relationship between the two characters -1. Thief gains Criminal trait.
            CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, -1);
            _characterInvolved.AddTrait(new Criminal());
        }
    }
    private void AssistedTheftCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    private void ThwartedTheftSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1);
            state.descriptionLog.AddToFillers(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_characterInvolved.faction, _characterInvolved.faction.name, LOG_IDENTIFIER.FACTION_1));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Mechanics**: Transfer item from target character to the thief.
        TransferItem(_targetCharacter, _characterInvolved);

        //**Mechanics**: If same faction, 50% chance that personal relationship between the two characters -1 and Thief gains Criminal trait.
        if (_targetCharacter.faction.id == _characterInvolved.faction.id) {
            if (Random.Range(0, 100) < 50) {
                CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, -1);
                _characterInvolved.AddTrait(new Criminal());
            }
        }

        //**Level Up**: Thief Character +1
        _characterInvolved.LevelUp();
    }
    private void ThwartedTheftFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();
        //**Mechanics**: Relationship between the player factions and Faction 2 +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _targetCharacter.faction, 1, state);
    }
    private void ThwartedTheftCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            state.descriptionLog.AddToFillers(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter.faction, _targetCharacter.faction.name, LOG_IDENTIFIER.FACTION_2));

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();

        //**Level Up**: Diplomat Minion +1
        investigatorCharacter.LevelUp();

        //**Mechanics**: Relationship between the player factions and Faction 2 +1
        AdjustFactionsRelationship(PlayerManager.Instance.player.playerFaction, _targetCharacter.faction, 1, state);
    }
    private void NormalTheftSuccessRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Transfer item from target character to the thief.
        TransferItem(_targetCharacter, _characterInvolved);

        //**Mechanics**: If same faction, 50% chance that personal relationship between the two characters -1 and Thief gains Criminal trait.
        if (_targetCharacter.faction.id == _characterInvolved.faction.id) {
            if (Random.Range(0, 100) < 50) {
                CharacterManager.Instance.ChangePersonalRelationshipBetweenTwoCharacters(_targetCharacter, _characterInvolved, -1);
                _characterInvolved.AddTrait(new Criminal());
        }
    }

    //**Level Up**: Thief Character +1
    _characterInvolved.LevelUp();
    }
    private void NormalTheftFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void NormalTheftCriticalFailRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Mechanics**: Character Name 1 dies.
        _characterInvolved.Death();
    }
    #endregion

    private void TransferItem(Character sourceCharacter, Character thief) {
        thief.ObtainToken(sourceCharacter.tokenInInventory);
        sourceCharacter.UnobtainToken();
    }

    public void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    public Character GetTargetCharacter(Character characterInvolved) {
        /*
         Once the actual action is triggered, the character will randomly select any character in the location holding 
         an item that is not part of the character's faction. 
         If the character has Crooked trait, this will include characters from his own faction.
         */
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.tokenInInventory != null 
                && currCharacter.id != characterInvolved.id
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.minion == null) {
                if (currCharacter.faction.id == characterInvolved.faction.id) {
                    //check if the character involved is crooked
                    if (characterInvolved.GetTrait("Crooked") == null) {
                        continue; //skip
                    }
                }
                choices.Add(currCharacter);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
