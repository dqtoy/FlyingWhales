using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class TakeItem : PlayerAbility {

    public TakeItem() : base() {
        _name = "Take Item";
        _description = "Take item from a structure";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (interactable is BaseLandmark) {
            BaseLandmark landmark = interactable as BaseLandmark;
            if(landmark.currentlySelectedItemInLandmark != null) {
                PlayerManager.Instance.player.AddItem(landmark.currentlySelectedItemInLandmark);
                landmark.RemoveItemInLandmark(landmark.currentlySelectedItemInLandmark);
                landmark.SetCurrentlySelectedItemInLandmark(null);
                base.Activate(interactable);
            }
        }
    }
    #endregion
}
