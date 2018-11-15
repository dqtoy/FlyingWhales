using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class RevealSecret : PlayerAbility {

    public RevealSecret() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Reveal Secret";
        _description = "Reveal a character's secret";
        _powerCost = 20;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void DoAbility(IInteractable interactable) {
        //Character character = interactable as Character;
        //character.currentlySelectedSecret.RevealSecret();
        //base.Activate(interactable);
        Character character = interactable as Character;
        //Activate(interactable, character.currentlySelectedSecret);
        RecallMinion();
    }
    public void Activate(IInteractable interactable, Secret secret) {
        secret.RevealSecret();
        //base.Activate(interactable);
    }
    public override bool CanBeDone(IInteractable interactable) {
        //if (base.CanBeDone(interactable)) {
        //Character character = interactable as Character;
        //if (character.currentlySelectedSecret != null && !character.currentlySelectedSecret.isRevealed) {
        //    return true;
        //}
        //}
        //Character character = interactable as Character;
        //return CanBeDone(interactable, character.currentlySelectedSecret);
        return true;
    }
    public bool CanBeDone(IInteractable interactable, Secret secret) {
        if (base.CanBeDone(interactable)) {
            if (secret != null && !secret.isRevealed) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
