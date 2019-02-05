using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealActionFaction : Interaction {

    private Character _targetCharacter;
    
    private const string Normal_Theft_Success = "Normal Theft Success";
    private const string Normal_Theft_Fail = "Normal Theft Fail";
    private const string Normal_Theft_Critical_Fail = "Normal Theft Critical Fail";

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public StealActionFaction(Area interactable)
        : base(interactable, INTERACTION_TYPE.STEAL_ACTION_FACTION, 0) {
        _name = "Steal Action Faction";
        //_jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalTheftSuccess = new InteractionState(Normal_Theft_Success, this);
        InteractionState normalTheftFail = new InteractionState(Normal_Theft_Fail, this);
        InteractionState normalTheftCriticalFail = new InteractionState(Normal_Theft_Critical_Fail, this);

        SetTargetCharacter(GetTargetCharacter(_characterInvolved));

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(null, _targetCharacter.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        normalTheftSuccess.SetEffect(() => NormalTheftSuccessRewardEffect(normalTheftSuccess));
        normalTheftFail.SetEffect(() => NormalTheftFailRewardEffect(normalTheftFail));
        normalTheftCriticalFail.SetEffect(() => NormalTheftCriticalFailRewardEffect(normalTheftCriticalFail));

        _states.Add(startState.name, startState);

        _states.Add(normalTheftSuccess.name, normalTheftSuccess);
        _states.Add(normalTheftFail.name, normalTheftFail);
        _states.Add(normalTheftCriticalFail.name, normalTheftCriticalFail);

        SetCurrentState(startState);
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
        if (GetTargetCharacter(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter) {
        this._targetCharacter = targetCharacter;
    }
    #endregion

    #region Option Effect
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
    public Character GetTargetCharacter(Character characterInvolved) {
        /*
         Once the actual action is triggered, the character will randomly select any character in the location holding 
         an item that is not part of the character's faction. 
         */
        List<Character> choices = new List<Character>();
        for (int i = 0; i < interactable.charactersAtLocation.Count; i++) {
            Character currCharacter = interactable.charactersAtLocation[i];
            if (currCharacter.tokenInInventory != null 
                && currCharacter.id != characterInvolved.id
                && !currCharacter.currentParty.icon.isTravelling
                && currCharacter.minion == null) {
                choices.Add(currCharacter);
            }
        }
        if (choices.Count > 0) {
            return choices[Random.Range(0, choices.Count)];
        }
        return null;
    }
}
