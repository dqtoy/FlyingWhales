using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Provoke : PlayerJobAction {

    public Provoke() {
        name = "Provoke";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        PlayerUI.Instance.OpenProvoke(assignedCharacter, targetCharacter);
        base.ActivateAction(assignedCharacter, targetCharacter);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) {
            return false;
        }
        //if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.faction.id == FactionManager.Instance.neutralFaction.id) {
        //    return false;
        //}
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.faction.id == FactionManager.Instance.neutralFaction.id) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
