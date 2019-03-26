using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intervene : PlayerJobAction {

    public Intervene() {
        actionName = "Intervene";
        SetDefaultCooldownTime(48);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        base.ActivateAction(assignedCharacter, targetCharacter);
        //targetCharacter.plannedInteraction.SetIsPrevented(true);
        //targetCharacter.OnInteractionEnded(targetCharacter.plannedInteraction);
        //UIManager.Instance.characterInfoUI.UpdateBasicInfo();
        if (targetCharacter.isDead || targetCharacter.currentAction == null) {
            return;
        }
        targetCharacter.currentAction.StopAction();
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id || targetCharacter.currentAction == null) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
}
