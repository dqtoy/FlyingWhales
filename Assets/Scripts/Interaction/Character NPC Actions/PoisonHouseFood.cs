using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonHouseFood : Interaction {

    private const string Poisoning_Cancelled = "Poisoning Cancelled";
    private const string Poisoning_Continues = "Poisoning Continues";
    private const string Character_Poisons = "Character Poisons";

    private Character _targetCharacter;

    public override Character targetCharacter {
        get { return _targetCharacter; }
    }

    public PoisonHouseFood(Area interactable)
        : base(interactable, INTERACTION_TYPE.POISON_HOUSE_FOOD, 0) {
        _name = "Poison House Food";
        _jobFilter = new JOB[] { JOB.DEBILITATOR };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState poisoningCancelled = new InteractionState(Poisoning_Cancelled, this);
        InteractionState poisoningContinues = new InteractionState(Poisoning_Continues, this);
        InteractionState characterPoisons = new InteractionState(Character_Poisons, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        poisoningCancelled.SetEffect(() => PoisoningCancelledRewardEffect(poisoningCancelled));
        poisoningContinues.SetEffect(() => PoisoningContinuesRewardEffect(poisoningContinues));
        characterPoisons.SetEffect(() => CharacterPoisonsRewardEffect(characterPoisons));

        _states.Add(startState.name, startState);
        _states.Add(poisoningCancelled.name, poisoningCancelled);
        _states.Add(poisoningContinues.name, poisoningContinues);
        _states.Add(characterPoisons.name, characterPoisons);

        SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from poisoning food.",
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
        if (_targetCharacter.homeArea.id != character.specificLocation.id) {
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
                nextState = Poisoning_Cancelled;
                break;
            case RESULT.FAIL:
                nextState = Poisoning_Continues;
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Character_Poisons]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_targetCharacter.homeStructure);
    }
    private void PoisoningCancelledRewardEffect(InteractionState state) {
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void PoisoningContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Add https://trello.com/c/waFphC2I/1180-poisoned-food Trait to the current Structure
        _targetCharacter.homeStructure.AddTrait("Poisoned Food");
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    private void CharacterPoisonsRewardEffect(InteractionState state) {
        //**Mechanics**: Add https://trello.com/c/waFphC2I/1180-poisoned-food Trait to the current Structure
        _targetCharacter.homeStructure.AddTrait("Poisoned Food");
        state.descriptionLog.AddToFillers(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
        state.AddLogFiller(new LogFiller(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER));
    }
    #endregion
}
