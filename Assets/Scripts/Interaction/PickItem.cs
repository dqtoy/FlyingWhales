using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItem : Interaction {

    WeightedDictionary<SpecialToken> pickWeights = new WeightedDictionary<SpecialToken>();

    public PickItem(BaseLandmark interactable) : base(interactable, INTERACTION_TYPE.PICK_ITEM, 0) {
        _name = "Pick Item";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        startState.SetEffect(() => StartRewardEffect(startState));

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.tokenInInventory != null) {
            return false;
        } else {
            for (int i = 0; i < interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count; i++) {
                SpecialToken token = interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns[i];
                if (token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                    pickWeights.AddElement(token, 60);
                } else if (token.CanBeUsedBy(character)) {
                    pickWeights.AddElement(token, 100);
                }
            }
            if (pickWeights.Count <= 0) {
                return false;
            }
        }
        return base.CanInteractionBeDoneBy(character);
    }
    //public override bool CanStillDoInteraction() {
    //    if (_characterInvolved.tokenInInventory != null) {
    //        return false;
    //    } else {
    //        for (int i = 0; i < interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns.Count; i++) {
    //            SpecialToken token = interactable.tileLocation.areaOfTile.possibleSpecialTokenSpawns[i];
    //            if (token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
    //                pickWeights.AddElement(token, 60);
    //            } else if (token.CanBeUsedBy(_characterInvolved)) {
    //                pickWeights.AddElement(token, 100);
    //            }
    //        }
    //        if (pickWeights.Count <= 0) {
    //            return false;
    //        }
    //    }
    //    return base.CanStillDoInteraction();
    //}
    #endregion

    private void StartRewardEffect(InteractionState state) {
        SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
        _characterInvolved.PickUpToken(chosenToken, interactable);

        //**Mechanics**: Character drops the item it is holding and leaves it at the location.
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
}
