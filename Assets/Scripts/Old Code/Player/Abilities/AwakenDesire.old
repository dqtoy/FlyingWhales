using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AwakenDesire : PlayerAbility {

    public AwakenDesire() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Awaken Hidden Desire";
        _description = "Awaken a hidden desire of character";
        _powerCost = 35;
        _threatGain = 10;
        _cooldown = 12;
    }

    #region Overrides
    public override void DoAbility(IInteractable interactable) {
        base.DoAbility(interactable);
        //interactable.hiddenDesire.Awaken();
        RecallMinion();
    }
    public void Activate(IInteractable interactable, ECS.Character character) {
        //character.hiddenDesire.Awaken();
        //base.Activate(interactable);
    }
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            //if (interactable.hiddenDesire != null && !interactable.hiddenDesire.isAwakened) {
            //    return true;
            //}
        }
        return false;
    }
    #endregion
}
