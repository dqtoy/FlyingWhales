using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessMemories : PlayerJobAction {

    public AccessMemories() : base(INTERVENTION_ABILITY.ACCESS_MEMORIES) {
        description = "Access the memories of a character.";
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        base.ActivateAction(targetCharacter);
        UIManager.Instance.ShowCharacterInfo(targetCharacter);
        PlayerUI.Instance.ShowMemories(targetCharacter);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_access_memory");
        log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
