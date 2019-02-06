using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemOnLocation : Interaction {

    private SpecialToken _tokenToBeUsed;

    private const string Stop_Successful = "Stop Successful";
    private const string Do_Nothing = "Do nothing";


    public UseItemOnLocation(Area interactable) : base(interactable, INTERACTION_TYPE.USE_ITEM_ON_LOCATION, 0) {
        _name = "Use Item On Location";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    public void SetItemToken(SpecialToken specialToken) {
        _tokenToBeUsed = specialToken;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState stopSuccessful = new InteractionState(Stop_Successful, this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "_description", this);
        startStateDescriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
        startState.OverrideDescriptionLog(startStateDescriptionLog);

        stopSuccessful.SetEffect(() => StopSuccessfulRewardEffect(stopSuccessful));
        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(stopSuccessful.name, stopSuccessful);
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
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
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
                _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, interactable);
                nextState = _tokenToBeUsed.Stop_Fail;
                break;
            default:
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, interactable);
        if (!_states.ContainsKey(_tokenToBeUsed.Item_Used)) {
            throw new System.Exception(this.name + " does have state " + _tokenToBeUsed.Item_Used + " when using token " + _tokenToBeUsed.name);
        }
        SetCurrentState(_states[_tokenToBeUsed.Item_Used]);
    }
    #endregion

    #region Reward Effect
    private void StopSuccessfulRewardEffect(InteractionState state) {
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1));

        //**Level Up**: Dissuader Minion +1
        //investigatorCharacter.LevelUp();
    }
    private void DoNothingRewardEffect(InteractionState state) {
        //_tokenToBeUsed.CreateJointInteractionStates(this);
    }
    #endregion
}
