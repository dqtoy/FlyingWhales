using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemOnStructure : Interaction {

    private SpecialToken _tokenToBeUsed;

    private const string Do_Nothing = "Do nothing";

    public UseItemOnStructure(Area interactable) : base(interactable, INTERACTION_TYPE.USE_ITEM_ON_STRUCTURE, 0) {
        _name = "Use Item On Structure";
    }

    public void SetItemToken(SpecialToken specialToken) {
        _tokenToBeUsed = specialToken;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
        InteractionState doNothing = new InteractionState(Do_Nothing, this);

        CreateActionOptions(startState);

        doNothing.SetEffect(() => DoNothingRewardEffect(doNothing));

        _states.Add(startState.name, startState);
        _states.Add(doNothing.name, doNothing);

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
        //if (!character.isHoldingItem || character.tokenInInventory != _tokenToBeUsed) {
        //    return false;
        //}
        return base.CanInteractionBeDoneBy(character);
    }
    #endregion

    #region Option Effects
    private void DoNothingOptionEffect(InteractionState state) {
        _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, _characterInvolved.currentStructure);
        if (!_states.ContainsKey(_tokenToBeUsed.Item_Used)) {
            throw new System.Exception(this.name + " does have state " + _tokenToBeUsed.Item_Used + " when using token " + _tokenToBeUsed.name);
        }
        SetCurrentState(_states[_tokenToBeUsed.Item_Used]);
    }
    #endregion

    #region Reward Effect
    private void DoNothingRewardEffect(InteractionState state) {
        //_tokenToBeUsed.CreateJointInteractionStates(this);
    }
    #endregion
}
