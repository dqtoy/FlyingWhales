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
    public override void DoAbility(IInteractable interactable) {
        base.DoAbility(interactable);
        Item dragonEgg = PlayerManager.Instance.player.GetItem("Dragon Egg");
        Character character = interactable as Character;
        character.AddAttribute(ATTRIBUTE.MARKED);
        PlayerManager.Instance.player.RemoveItem(dragonEgg);
        RecallMinion();
    }
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            if (PlayerManager.Instance.player.GetItem("Dragon Egg") != null) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
