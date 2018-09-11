using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ShareIntel : PlayerAbility {

    public ShareIntel() : base() {
        _name = "Share Intel";
        _description = "Share intel to a character which may or may not change his/her behaviour";
        _powerCost = 5;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if(interactable is Character) {
            Character character = interactable as Character;
            PlayerManager.Instance.player.PickIntelToGiveToCharacter(character, this);
        }
    }
    #endregion

    public void HasGivenIntel(IInteractable interactable) {
        base.Activate(interactable);
    }


}
