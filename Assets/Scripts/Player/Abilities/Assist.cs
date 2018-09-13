using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Assist : PlayerAbility {

    public Assist() : base() {
        _name = "Assist";
        _description = "Assist a character pn the current action";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (interactable is Character) {
            Character character = interactable as Character;
            character.party.actionData.SetIsBeingAssisted(true);
            base.Activate(interactable);
        }
    }
    #endregion
}
