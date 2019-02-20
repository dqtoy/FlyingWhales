using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Track : PlayerJobAction {

    //public object target { get; private set; }
    //public JOB_ACTION_TARGET currentTargetType { get; private set; }

    public Track() {
        actionName = "Track";
        SetDefaultCooldownTime(90);
        //currentTargetType = JOB_ACTION_TARGET.NONE;
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    //public override void ActivateAction(Character assignedCharacter, Area targetArea) {
    //    base.ActivateAction(assignedCharacter, targetArea);
    //    currentTargetType = JOB_ACTION_TARGET.AREA;
    //    target = targetArea;
    //    targetArea.SetTrackedState(true);
    //    Debug.Log(GameManager.Instance.TodayLogString() + assignedCharacter.name + " is now tracking " + targetArea.name);
    //    SetSubText("Currently tracking " + targetArea.name);
    //}
    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        base.ActivateAction(assignedCharacter, targetCharacter);
        //currentTargetType = JOB_ACTION_TARGET.CHARACTER;
        //target = targetCharacter;
        //targetCharacter.ownParty.icon.SetVisualState(true);
        targetCharacter.SetTracked(true);
        Debug.Log(GameManager.Instance.TodayLogString() + assignedCharacter.name + " is now tracking " + targetCharacter.name);
        //SetSubText("Currently tracking " + targetCharacter.name);
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        //if (target is Character) {
        //    (target as Character).ownParty.icon.SetVisualState(false);
        //} else 
        //if (target is Area) {
        //    (target as Area).SetTrackedState(false);
        //}
        //currentTargetType = JOB_ACTION_TARGET.NONE;
        //target = null;
        //SetSubText(string.Empty);
    }
    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (character.id == targetCharacter.id) {
            return false;
        }
        if (targetCharacter.isTracked) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }
    //protected override bool ShouldButtonBeInteractable(Character character, Area targetArea) {
    //    if (currentTargetType != JOB_ACTION_TARGET.NONE) {
    //        if (target is Area && targetArea.id == (target as Area).id) {
    //            return false;
    //        }
    //    }
    //    return base.ShouldButtonBeInteractable(character, targetArea);
    //}
    //protected override void OnCharacterDied(Character characterThatDied) {
    //    base.OnCharacterDied(characterThatDied);
    //    if (!this.isActive) {
    //        return; //if this action is no longer active, do not check if the character that died was the target
    //    }
    //    if (currentTargetType == JOB_ACTION_TARGET.CHARACTER) {
    //        if (characterThatDied.id == (target as Character).id) {
    //            DeactivateAction();
    //        }
    //    }
        
    //}
}
