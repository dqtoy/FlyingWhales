using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RestAtHomeAction : Interaction {

    private const string Character_Rests = "Character Rests";

    public RestAtHomeAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.REST_AT_HOME_ACTION, 0) {
        _name = "Rest At Home Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState characterRests = new InteractionState(Character_Rests, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        characterRests.SetEffect(() => CharacterRestsRewardEffect(characterRests));

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
                effect = () => DoNothingOptionEffect(state),
            };
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
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Character_Rests]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        _characterInvolved.MoveToAnotherStructure(_characterInvolved.homeStructure);
    }
    private void CharacterRestsRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    #endregion
}
