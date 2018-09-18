using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class TakeItem : PlayerAbility {

    public TakeItem() : base(ABILITY_TYPE.STRUCTURE) {
        _name = "Take Item";
        _description = "Take item from a structure";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
        BaseLandmark landmark = interactable as BaseLandmark;
        PlayerManager.Instance.player.PickItemToTakeFromLandmark(landmark, this);
    }
    #endregion

    public void HasTakenItem(IInteractable interactable) {
        base.Activate(interactable);
    }
}
