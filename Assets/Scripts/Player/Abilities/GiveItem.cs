using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class GiveItem : PlayerAbility {

    public GiveItem() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Give Item";
        _description = "Give item to a character";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
        Character character = interactable as Character;
        PlayerManager.Instance.player.PickItemToGiveToCharacter(character, this);
    }
    #endregion

    public void HasGivenItem(IInteractable interactable) {
        base.Activate(interactable);
    }
}
