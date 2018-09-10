using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inspect : PlayerAbility {

	public Inspect(IInteractable interactable): base(interactable) {
        _name = "Inspect";
        _description = "Inspect a structure, character, or monster";
        _powerCost = 5;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate() {

        base.Activate();
    }
    #endregion
}
