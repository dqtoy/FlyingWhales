using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestAtInn : Interaction {
    private const string Start = "Start";
    private const string Character_Rests = "Character Rests";

    public RestAtInn(Area interactable): base(interactable, INTERACTION_TYPE.REST_AT_INN, 0) {
        _name = "Rest At Inn";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState(Start, this);
        InteractionState characterRests = new InteractionState(Character_Rests, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);
        characterRests.SetEffect(() => CharacterRestsEffect(characterRests));

        _states.Add(startState.name, startState);
        _states.Add(characterRests.name, characterRests);

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
        SetCurrentState(_states[Character_Rests]);
    }
    #endregion

    #region State Effects
    private void StartEffect(InteractionState state) {
        _characterInvolved.MoveToRandomStructureInArea(STRUCTURE_TYPE.INN);
    }
    private void CharacterRestsEffect(InteractionState state) {
        _characterInvolved.ResetFullnessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    #endregion
}
