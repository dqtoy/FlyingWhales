using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Awaken : PlayerAbility {

    public Awaken() : base(ABILITY_TYPE.MONSTER) {
        _name = "Awaken";
        _description = "Awaken a monster from deep slumber";
        _powerCost = 25;
        _threatGain = 5;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
    }
    #endregion
}
