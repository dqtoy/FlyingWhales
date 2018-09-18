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
        Character character = interactable as Character;
        character.AddAttribute(ATTRIBUTE.SPOOKED);
        base.Activate(interactable);
    }
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            Character character = interactable as Character;
            if (character.GetAttribute(ATTRIBUTE.SPOOKED) == null) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
