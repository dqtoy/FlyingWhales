﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intervene : PlayerJobAction {

    public Intervene() {
        name = "Intervene";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        base.ActivateAction(assignedCharacter, targetPOI);
        //targetCharacter.plannedInteraction.SetIsPrevented(true);
        //targetCharacter.OnInteractionEnded(targetCharacter.plannedInteraction);
        //UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        if (targetCharacter.isDead || targetCharacter.currentAction == null) {
            return;
        }
        targetCharacter.currentAction.StopAction(true);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id || targetCharacter.currentAction == null) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }

    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead || assignedCharacter == targetCharacter || targetCharacter.currentAction == null) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
