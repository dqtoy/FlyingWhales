using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Spook : PlayerAbility {

    public Spook() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Spook";
        _description = "Spook a character into leaving the location";
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
        if (character.AddAttribute(ATTRIBUTE.SPOOKED) != null) {
            base.Activate(interactable);
        }
    }
    #endregion
}
