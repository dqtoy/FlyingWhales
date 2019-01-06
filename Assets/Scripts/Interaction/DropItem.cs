using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : Interaction {

    public DropItem(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.DROP_ITEM, 0) {
        _name = "Drop Item";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        startState.SetEffect(() => StartRewardEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    public override bool CanStillDoInteraction() {
        if (_characterInvolved.tokenInInventory == null) {
            return false;
        }
        return base.CanStillDoInteraction();
    }
    #endregion

    private void StartRewardEffect(InteractionState state) {
        //**Mechanics**: Character drops the item it is holding and leaves it at the location.
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
        _characterInvolved.DropToken(interactable);
    }
}
