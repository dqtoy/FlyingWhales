using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemOnCharacter : Interaction {

    private SpecialToken _tokenToBeUsed;

    private const string Stop_Successful = "Stop Successful";
    //private const string Stop_Fail = "Stop Fail";
    private const string Do_Nothing = "Do nothing";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public UseItemOnCharacter(Area interactable) : base(interactable, INTERACTION_TYPE.USE_ITEM_ON_CHARACTER, 0) {
        _name = "Use Item On Character";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetItemToken(SpecialToken specialToken) {
        _tokenToBeUsed = specialToken;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stopSuccessful = new InteractionState(Stop_Successful, this);
        //InteractionState stopFail = new InteractionState(Stop_Fail, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        _targetCharacter = _tokenToBeUsed.GetTargetCharacterFor(_characterInvolved);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
        startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        CreateActionOptions(startState);

        stopSuccessful.SetEffect(() => StopSuccessfulRewardEffect(stopSuccessful));
        //stopFail.SetEffect(() => StopFailRewardEffect(stopFail));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(stopSuccessful.name, stopSuccessful);
        //_states.Add(stopFail.name, stopFail);
        _states.Add(doNothing.name, doNothing);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption stop = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Stop " + _characterInvolved.name + ".",
                effect = () => StopOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have dissuader minion."
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(stop);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.tokenInInventory == null 
            || character.tokenInInventory != _tokenToBeUsed 
            || _tokenToBeUsed.GetTargetCharacterFor(character) == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effects
    private void StopOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Stop_Successful;
                break;
            case RESULT.FAIL:
                _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, _targetCharacter);
                nextState = _tokenToBeUsed.Stop_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, _targetCharacter);
        if (!_states.ContainsKey(_tokenToBeUsed.Item_Used)) {
            throw new System.Exception(this.name + " does have state " + _tokenToBeUsed.Item_Used + " when using token " + _tokenToBeUsed.name);
        }
        SetCurrentState(_states[_tokenToBeUsed.Item_Used]);
        //SetCurrentState(_states[Do_Nothing]);
    }
    #endregion

    #region Reward Effect
    private void StopSuccessfulRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
            state.descriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        }
        state.AddLogFiller(new LogFiller(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        //**Level Up**: Dissuader Minion +1
        investigatorCharacter.LevelUp();
    }
    private void StopFailRewardEffect(InteractionState state) {        
        //if (state.descriptionLog != null) {
        //    state.descriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
        //    state.descriptionLog.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //}
        //state.AddLogFiller(new LogFiller(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1));
        //state.AddLogFiller(new LogFiller(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));

        ////**Level Up**: Dissuader Minion +1
        //investigatorMinion.LevelUp();
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //_tokenToBeUsed.CreateJointInteractionStates(this);
    }
    #endregion
}
