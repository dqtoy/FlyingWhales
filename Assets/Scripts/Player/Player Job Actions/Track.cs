using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : PlayerJobAction {

    private object target;
    private JOB_ACTION_TARGET currentTargetType;

    public Track() {
        actionName = "Track";
        SetDefaultCooldownTime(90);
        currentTargetType = JOB_ACTION_TARGET.NONE;
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER, JOB_ACTION_TARGET.AREA };
    }

    public override void ActivateAction(Character assignedCharacter, Area targetArea) {
        base.ActivateAction(assignedCharacter, targetArea);
        currentTargetType = JOB_ACTION_TARGET.AREA;
        target = targetArea;
        SetSubText("Currently tracking " + targetArea.name);
    }
    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        currentTargetType = JOB_ACTION_TARGET.CHARACTER;
        target = targetCharacter;
        targetCharacter.ownParty.icon.SetVisualState(true);
        base.ActivateAction(assignedCharacter, targetCharacter);
        Debug.Log(GameManager.Instance.TodayLogString() + assignedCharacter.name + " is now tracking " + targetCharacter.name);
        SetSubText("Currently tracking " + targetCharacter.name);
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        if (target is Character) {
            (target as Character).ownParty.icon.SetVisualState(false);
        }
        currentTargetType = JOB_ACTION_TARGET.NONE;
        target = null;
        SetSubText(string.Empty);
    }
    public override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id) {
            return false;
        }
        if (currentTargetType != JOB_ACTION_TARGET.NONE) {
            if (target is Character && targetCharacter.id == (target as Character).id) {
                return false;
            }
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    protected override void OnCharacterDied(Character characterThatDied) {
        base.OnCharacterDied(characterThatDied);
        if (!this.isActive) {
            return; //if this action is no longer active, do not check if the character that died was the target
        }
        if (currentTargetType == JOB_ACTION_TARGET.CHARACTER) {
            if (characterThatDied.id == (target as Character).id) {
                DeactivateAction();
            }
        }
        
    }
}
