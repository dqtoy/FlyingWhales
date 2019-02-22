using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampOutAction : Interaction {

    private const string Camp_Cancelled = "Camp Cancelled";
    private const string Camp_Continues = "Camp Continues";
    private const string Normal_Camp = "Normal Camp";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public CampOutAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.CAMP_OUT_ACTION, 0) {
        _name = "Camp Out Action";
        _jobFilter = new JOB[] { JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState campCancelled = new InteractionState(Camp_Cancelled, this);
        InteractionState campContinues = new InteractionState(Camp_Continues, this);
        InteractionState normalCamp = new InteractionState(Normal_Camp, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        campCancelled.SetEffect(() => CampCancelledRewardEffect(campCancelled));
        campContinues.SetEffect(() => CampContinuesRewardEffect(campContinues));
        normalCamp.SetEffect(() => NormalCampRewardEffect(normalCamp));

        _states.Add(startState.name, startState);
        _states.Add(campCancelled.name, campCancelled);
        _states.Add(campContinues.name, campContinues);
        _states.Add(normalCamp.name, normalCamp);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from camping out.",
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
        if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character character) {
        _targetCharacter = character;
    }
    #endregion

    #region Option Effect
    private void PreventFromLeavingOptionEffect(InteractionState state) {
        WeightedDictionary<RESULT> resultWeights = investigatorCharacter.job.GetJobRateWeights();
        resultWeights.RemoveElement(RESULT.CRITICAL_FAIL);

        string nextState = string.Empty;
        switch (resultWeights.PickRandomElementGivenWeights()) {
            case RESULT.SUCCESS:
                nextState = Camp_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Camp_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Camp]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        //**Structure**: Move the character to a random Wilderness structure
        _characterInvolved.MoveToAnotherStructure(interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
    }
    private void CampCancelledRewardEffect(InteractionState state) {
        
    }
    private void CampContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    private void NormalCampRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    #endregion
}
