using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EatInnMeal : Interaction {
    private const string Start = "Start";
    private const string Character_Clean_Eats = "Character Clean Eats";
    private const string Character_Eats_Killed = "Character Eats Killed";
    private const string Character_Eats_Sick = "Character Eats Sick";

    public EatInnMeal(Area interactable): base(interactable, INTERACTION_TYPE.EAT_INN_MEAL, 0) {
        _name = "Eat Inn Meal";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState characterCleanEats = new InteractionState(Character_Clean_Eats, this);
        InteractionState characterEatsKilled = new InteractionState(Character_Eats_Killed, this);
        InteractionState characterEatsSick = new InteractionState(Character_Eats_Sick, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        characterCleanEats.SetEffect(() => CharacterCleanEatsEffect(characterCleanEats));
        characterEatsKilled.SetEffect(() => CharacterEatsKilledEffect(characterEatsKilled));
        characterEatsSick.SetEffect(() => CharacterEatsSickEffect(characterEatsSick));

        _states.Add(startState.name, startState);
        _states.Add(characterCleanEats.name, characterCleanEats);
        _states.Add(characterEatsKilled.name, characterEatsKilled);
        _states.Add(characterEatsSick.name, characterEatsSick);

        //SetCurrentState(startState);
    }
    public override void CreateActionOptions(InteractionState state) {
        if (state.name == "Start") {
            ActionOption doNothing = new ActionOption {
                interactionState = state,
                cost = new CurrenyCost { amount = 0, currency = CURRENCY.SUPPLY },
                name = "Do nothing.",
                effect = () => DoNothingOptionEffect(),
            };
            state.AddActionOption(doNothing);
            state.SetDefaultOption(doNothing);
        }
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        //if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
        //    return false;
        //}
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect() {
        //if(_characterInvolved.currentStructure.GetTrait("Poisoned Food") != null) {
        //    WeightedDictionary<string> resultWeights = new WeightedDictionary<string>();
        //    resultWeights.AddElement(Character_Eats_Killed, 30);
        //    resultWeights.AddElement(Character_Eats_Sick, 20);
        //    string result = resultWeights.PickRandomElementGivenWeights();
        //    SetCurrentState(_states[result]);
        //} else {
        //    SetCurrentState(_states[Character_Clean_Eats]);
        //}
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.INN);
    }
    private void CharacterCleanEatsEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
        _characterInvolved.AddTrait("Eating");
    }
    private void CharacterEatsKilledEffect(InteractionState state) {
        //_characterInvolved.currentStructure.RemoveTrait("Poisoned Food");
        _characterInvolved.Death("poison");
    }
    private void CharacterEatsSickEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
        //_characterInvolved.currentStructure.RemoveTrait("Poisoned Food");
        _characterInvolved.AddTrait("Sick");
        _characterInvolved.AddTrait("Eating");
    }
    #endregion
}
