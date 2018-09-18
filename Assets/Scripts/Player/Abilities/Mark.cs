using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Mark : PlayerAbility {

    public Mark() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Mark";
        _description = "Marks a character";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
        Item dragonEgg = PlayerManager.Instance.player.GetItem("Dragon Egg");
        if(dragonEgg == null) {
            return;
        }

        Character character = interactable as Character;
        character.AddAttribute(ATTRIBUTE.MARKED);
        PlayerManager.Instance.player.RemoveItem(dragonEgg);
        base.Activate(interactable);
    }
    #endregion
}
