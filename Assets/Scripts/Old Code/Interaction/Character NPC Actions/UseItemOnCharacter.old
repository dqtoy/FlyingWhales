using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseItemOnCharacter : Interaction {

    private SpecialToken _tokenToBeUsed;

    public UseItemOnCharacter(Area interactable) : base(interactable, INTERACTION_TYPE.USE_ITEM_ON_CHARACTER, 0) {
        _name = "Use Item On Character";
    }

    public void SetItemToken(SpecialToken specialToken) {
        _tokenToBeUsed = specialToken;
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);
    
        _tokenToBeUsed.CreateJointInteractionStates(this, _characterInvolved, _targetCharacter);

        if (_characterInvolved.id == _targetCharacter.id) {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-self_description", this);
            startStateDescriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        } else {
            Log startStateDescriptionLog = new Log(GameManager.Instance.Today(), "Events", this.GetType().ToString(), startState.name.ToLower() + "-other_description", this);
            startStateDescriptionLog.AddToFillers(null, _tokenToBeUsed.nameInBold, LOG_IDENTIFIER.STRING_1);
            startStateDescriptionLog.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            startState.OverrideDescriptionLog(startStateDescriptionLog);
        }
        CreateActionOptions(startState);

        startState.SetEffect(() => StartEffect(startState), false);

        _states.Add(startState.name, startState);

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
        //} else 
        if(_targetCharacter == null) {
            return false;
        }
        return base.CanInteractionBeDoneBy(character);
    }
    public override void SetTargetCharacter(Character targetCharacter, Character actor) {
        _targetCharacter = targetCharacter;
    }
    #endregion

    #region Option Effects
    private void DoNothingOptionEffect(InteractionState state) {
        if (!_states.ContainsKey(_tokenToBeUsed.Item_Used)) {
            throw new System.Exception(this.name + " does have state " + _tokenToBeUsed.Item_Used + " when using token " + _tokenToBeUsed.name);
        }
        SetCurrentState(_states[_tokenToBeUsed.Item_Used]);
    }
    #endregion

    #region Reward Effect
    private void StartEffect(InteractionState state) {
        _tokenToBeUsed.StartTokenInteractionState(_characterInvolved, _targetCharacter);
    }
    #endregion
}
