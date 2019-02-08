﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickItem : Interaction {

    List<SpecialToken> items = new List<SpecialToken>();

    private SpecialToken targetToken;
    private LocationStructure _targetStructure;
    public override LocationStructure actionStructureLocation {
        get { return _targetStructure; }
    }

    public PickItem(Area interactable) : base(interactable, INTERACTION_TYPE.PICK_ITEM, 0) {
        _name = "Pick Item";
    }

    #region Overrides
    public override void CreateStates() {
        InteractionState startState = new InteractionState("Start", this);

        startState.SetEffect(() => StartRewardEffect(startState));
        targetToken = items[UnityEngine.Random.Range(0, items.Count)];

        _states.Add(startState.name, startState);
        SetCurrentState(startState);
    }
    public override bool CanInteractionBeDoneBy(Character character) {
        if (character.isHoldingItem) {
            return false;
        } else {
            for (int i = 0; i < interactable.possibleSpecialTokenSpawns.Count; i++) {
                SpecialToken token = interactable.possibleSpecialTokenSpawns[i];
                if(token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE || 
                    (character.isAtHomeStructure && token.structureLocation == character.homeStructure)) {
                    items.Add(token);
                    //if (token.npcAssociatedInteractionType != INTERACTION_TYPE.USE_ITEM_ON_SELF) {
                    //    pickWeights.AddElement(token, 60);
                    //} else if (token.CanBeUsedBy(character)) {
                    //    pickWeights.AddElement(token, 100);
                    //}
                }
            }
            if (items.Count <= 0) {
                return false;
            }
        }
        return base.CanInteractionBeDoneBy(character);
    }
    //public override bool CanStillDoInteraction() {
    //    if (_characterInvolved.tokenInInventory != null) {
    //        return false;
    //    } else {
    //        for (int i = 0; i < interactable.possibleSpecialTokenSpawns.Count; i++) {
    //            SpecialToken token = interactable.possibleSpecialTokenSpawns[i];
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
    public override object GetTarget() {
        return targetToken;
    }
    #endregion

    private void StartRewardEffect(InteractionState state) {
        _targetStructure = targetToken.structureLocation;
        _characterInvolved.MoveToAnotherStructure(_targetStructure);
        //SpecialToken chosenToken = pickWeights.PickRandomElementGivenWeights();
        _characterInvolved.PickUpToken(targetToken, interactable);

        //**Mechanics**: Character drops the item it is holding and leaves it at the location.
        if (state.descriptionLog != null) {
            state.descriptionLog.AddToFillers(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1);
        }
        state.AddLogFiller(new LogFiller(null, _characterInvolved.tokenInInventory.nameInBold, LOG_IDENTIFIER.STRING_1));
    }
}
