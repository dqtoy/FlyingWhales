using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessMemories : PlayerJobAction {

    public AccessMemories() {
        name = "Access Memories";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        base.ActivateAction(assignedCharacter, targetCharacter);
        UIManager.Instance.ShowCharacterInfo(targetCharacter);
        PlayerUI.Instance.ShowMemories(targetCharacter);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_access_memory");
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (character.id == targetCharacter.id) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        if (assignedCharacter == targetCharacter) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
