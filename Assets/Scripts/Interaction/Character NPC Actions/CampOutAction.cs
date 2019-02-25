using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampOutAction : Interaction {

    private const string Normal_Camp = "Normal Camp";

    public CampOutAction(Area interactable)
        : base(interactable, INTERACTION_TYPE.CAMP_OUT_ACTION, 0) {
        _name = "Camp Out Action";
    }

    #region Override
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState normalCamp = new InteractionState(Normal_Camp, this);

        CreateActionOptions(startState);

        startState.SetEffect(() => StartRewardEffect(startState), false);
        normalCamp.SetEffect(() => NormalCampRewardEffect(normalCamp));

        _states.Add(startState.name, startState);
        _states.Add(normalCamp.name, normalCamp);

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
        if (!InteractionManager.Instance.CanCreateInteraction(type, character)) {
            return false;
        }
        _targetStructure = interactable.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effect
    private void DoNothingOptionEffect(InteractionState state) {
        SetCurrentState(_states[Normal_Camp]);
    }
    #endregion

    #region Reward Effect
    private void StartRewardEffect(InteractionState state) {
        //**Structure**: Move the character to a random Wilderness structure
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
    }
    private void NormalCampRewardEffect(InteractionState state) {
        //**Mechanics**: Fully replenish character's Tiredness meter. Add https://trello.com/c/VaLZqJKT/1190-resting trait to the character.
        _characterInvolved.ResetTirednessMeter();
        _characterInvolved.AddTrait("Resting");
    }
    #endregion
}
