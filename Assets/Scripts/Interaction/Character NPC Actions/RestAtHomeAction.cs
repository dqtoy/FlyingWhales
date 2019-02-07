using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestAtHomeAction : Interaction {

    private const string Rest_Cancelled = "Rest Cancelled";
    private const string Rest_Continues = "Rest Continues";
    private const string Character_Rests = "Character Rests";

    public RestAtHomeAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.REST_AT_HOME_ACTION, 0) {
        _name = "Rest At Home Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState restCancelled = new InteractionState(Rest_Cancelled, this);
        InteractionState restContinues = new InteractionState(Rest_Continues, this);
        InteractionState characterRests = new InteractionState(Character_Rests, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        restCancelled.SetEffect(() => RestCancelledRewardEffect(restCancelled));
        restContinues.SetEffect(() => RestContinuesRewardEffect(restContinues));
        characterRests.SetEffect(() => CharacterRestsRewardEffect(characterRests));

        _states.Add(startState.name, startState);
        _states.Add(restCancelled.name, restCancelled);
        _states.Add(restContinues.name, restContinues);
        _states.Add(characterRests.name, characterRests);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from resting at home.",
                duration = 0,
                effect = () => PreventFromLeavingOptionEffect(state),
                jobNeeded = JOB.DEBILITATOR,
                doesNotMeetRequirementsStr = "Must have debilitator minion.",
            };
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(state),
            };
            state.AddActionOption(prevent);
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.homeArea.id != character.specificLocation.id) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Rest_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Rest_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Character_Rests]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_characterInvolved.homeStructure);
    }
    private void RestCancelledRewardEffect(InteractionState state) {
        
    }
    private void RestContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    private void CharacterRestsRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    #endregion
}
