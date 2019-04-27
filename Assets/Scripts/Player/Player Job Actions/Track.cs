using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Track : PlayerJobAction {

    public Character target { get; private set; }

    public Track() {
        name = "Track";
        SetDefaultCooldownTime(48);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        Messenger.AddListener<EventPopup>(Signals.EVENT_POPPED_UP, OnEventPoppedUp);
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        base.ActivateAction(assignedCharacter, targetCharacter);
        target = targetCharacter;
        target.SetTracked(true);
        Debug.Log(GameManager.Instance.TodayLogString() + assignedCharacter.name + " is now tracking " + targetCharacter.name);
        SetSubText("Tracking " + targetCharacter.name);
    }
    public override void DeactivateAction() {
        base.DeactivateAction();
        if (target != null) {
            target.SetTracked(false);
        }
        target = null;
        SetSubText(string.Empty);
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
    public override bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (assignedCharacter == targetCharacter) {
            return false;
        }
        if (targetCharacter.isTracked) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    private void OnEventPoppedUp(EventPopup popup) {
        if (target != null) {
            if (popup.log.IsIncludedInFillers(target)) {
                Log log = popup.log;
                Messenger.Broadcast<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION, Utilities.LogReplacer(log), 5, () => ConvertToIntel(log, popup));
            }
        }
    }
    private void ConvertToIntel(Log log, EventPopup popup) {
        //InteractionIntel intel = log.ConvertToIntel();
        //PlayerManager.Instance.player.AddIntel(intel);
        //if (popup.isAlive) {
        //    popup.DestroyPopup();
        //}
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
