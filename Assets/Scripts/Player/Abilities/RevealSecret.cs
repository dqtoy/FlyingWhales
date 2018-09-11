using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RevealSecret : PlayerAbility {

    public RevealSecret() : base() {
        _name = "Reveal Secret";
        _description = "Reveal a character's secret";
        _powerCost = 20;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (interactable is Character) {
            Character character = interactable as Character;
            if(character.currentlySelectedSecret != null) {
                character.currentlySelectedSecret.RevealSecret();
                base.Activate(interactable);
            }
        }
    }
    #endregion
}
