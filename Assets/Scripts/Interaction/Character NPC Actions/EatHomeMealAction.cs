using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatHomeMealAction : Interaction {

    private const string Eat_Cancelled = "Eat Cancelled";
    private const string Clean_Eat_Continues = "Clean Eat Continues";
    private const string Eat_Continues_Killed = "Eat Continues Killed";
    private const string Eat_Continues_Sick = "Eat Continues Sick";
    private const string Character_Clean_Eats = "Character Clean Eats";
    private const string Character_Eats_Killed = "Character Eats Killed";
    private const string Character_Eats_Sick = "Character Eats Sick";

    public EatHomeMealAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.EAT_HOME_MEAL_ACTION, 0) {
        _name = "Eat Home Meal Action";
        _jobFilter = new JOB[] { JOB.INSTIGATOR, JOB.DIPLOMAT };
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState eatCancelled = new InteractionState(Eat_Cancelled, this);
        InteractionState cleanEatContinues = new InteractionState(Clean_Eat_Continues, this);
        InteractionState eatContinuesKilled = new InteractionState(Eat_Continues_Killed, this);
        InteractionState eatContinuesSick = new InteractionState(Eat_Continues_Sick, this);
        InteractionState characterCleanEats = new InteractionState(Character_Clean_Eats, this);
        InteractionState characterEatsKilled = new InteractionState(Character_Eats_Killed, this);
        InteractionState characterEatsSick = new InteractionState(Character_Eats_Sick, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        eatCancelled.SetEffect(() => EatCancelledRewardEffect(eatCancelled));
        cleanEatContinues.SetEffect(() => CleanEatContinuesRewardEffect(cleanEatContinues));
        eatContinuesKilled.SetEffect(() => EatContinuesKilledRewardEffect(eatContinuesKilled));
        eatContinuesSick.SetEffect(() => EatContinuesSickRewardEffect(eatContinuesSick));
        characterCleanEats.SetEffect(() => CharacterCleanEatsRewardEffect(characterCleanEats));
        characterEatsKilled.SetEffect(() => CharacterEatsKilledRewardEffect(characterEatsKilled));
        characterEatsSick.SetEffect(() => CharacterEatsSickRewardEffect(characterEatsSick));

        _states.Add(startState.name, startState);
        _states.Add(eatCancelled.name, eatCancelled);
        _states.Add(cleanEatContinues.name, cleanEatContinues);
        _states.Add(eatContinuesKilled.name, eatContinuesKilled);
        _states.Add(eatContinuesSick.name, eatContinuesSick);
        _states.Add(characterCleanEats.name, characterCleanEats);
        _states.Add(characterEatsKilled.name, characterEatsKilled);
        _states.Add(characterEatsSick.name, characterEatsSick);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption prevent = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Prevent " + Utilities.GetPronounString(_characterInvolved.gender, PRONOUN_TYPE.OBJECTIVE, false) + " from eating a meal.",
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
                nextState = Eat_Cancelled;
                break;
            case RESULT.FAIL:
                if (_characterInvolved.homeStructure.GetTrait("Poisoned Food") == null) {
                    nextState = Clean_Eat_Continues;
                } else {
                    WeightedDictionary<string> result = new WeightedDictionary<string>();
                    result.AddElement(Eat_Continues_Killed, 30);
                    result.AddElement(Eat_Continues_Sick, 20);
                    nextState = result.PickRandomElementGivenWeights();
                }
                break;
        }
        SetCurrentState(_states[nextState]);
    }
    private void DoNothingOptionEffect(InteractionState state) {
        string nextState = string.Empty;
        if (_characterInvolved.homeStructure.GetTrait("Poisoned Food") == null) {
            nextState = Character_Clean_Eats;
        } else {
            WeightedDictionary<string> result = new WeightedDictionary<string>();
            result.AddElement(Character_Eats_Killed, 30);
            result.AddElement(Character_Eats_Sick, 20);
            nextState = result.PickRandomElementGivenWeights();
        }
        SetCurrentState(_states[nextState]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_characterInvolved.homeStructure);
        if (_characterInvolved.isDead) {
            EndInteraction();
        }
    }
    private void EatCancelledRewardEffect(InteractionState state) {

    }
    private void CleanEatContinuesRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Fullness meter.
        _characterInvolved.ResetFullnessMeter();
    }
    private void EatContinuesKilledRewardEffect(InteractionState state) {
        _characterInvolved.homeStructure.RemoveTrait("Poisoned Food");
        //**Mechanics**: Character dies. Remove https://trello.com/c/waFphC2I/1180-poisoned-food trait from the structure
        _characterInvolved.Death();
    }
    private void EatContinuesSickRewardEffect(InteractionState state) {
        //**Mechanics**: Character gains https://trello.com/c/SVR4fnx1/1177-sick trait. Remove https://trello.com/c/waFphC2I/1180-poisoned-food trait from the structure
        _characterInvolved.AddTrait("Sick");
        _characterInvolved.homeStructure.RemoveTrait("Poisoned Food");
    }
    private void CharacterCleanEatsRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Fullness meter.
        _characterInvolved.ResetFullnessMeter();
    }
    private void CharacterEatsKilledRewardEffect(InteractionState state) {
        _characterInvolved.homeStructure.RemoveTrait("Poisoned Food");
        //**Mechanics**: Character dies. Remove https://trello.com/c/waFphC2I/1180-poisoned-food trait from the structure
        _characterInvolved.Death();
    }
    private void CharacterEatsSickRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Fullness meter. Character gains https://trello.com/c/SVR4fnx1/1177-sick trait. Remove https://trello.com/c/waFphC2I/1180-poisoned-food trait from the structure
        _characterInvolved.ResetFullnessMeter();
        _characterInvolved.AddTrait("Sick");
        _characterInvolved.homeStructure.RemoveTrait("Poisoned Food");
    }
    #endregion
}
