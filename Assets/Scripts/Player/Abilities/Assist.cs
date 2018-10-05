using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Assist : PlayerAbility {

    public Assist() : base(ABILITY_TYPE.CHARACTER) {
        _name = "Assist";
        _description = "Assist a character on the current action";
        _powerCost = 10;
        _threatGain = 2;
        _cooldown = 12;
    }

    #region Overrides
    public override void DoAbility(IInteractable interactable) {
        base.DoAbility(interactable);
        Character character = interactable as Character;
        character.party.actionData.SetIsBeingAssisted(true);
    }
    public override void CancelAbility(IInteractable interactable) {
        base.CancelAbility(interactable);
        Character character = interactable as Character;
        character.party.actionData.SetIsBeingAssisted(false);
    }
    #endregion
}
