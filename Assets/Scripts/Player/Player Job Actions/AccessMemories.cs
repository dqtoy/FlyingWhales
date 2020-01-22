using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccessMemories : PlayerSpell {

    public AccessMemories() : base(SPELL_TYPE.ACCESS_MEMORIES) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
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
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class AccessMemoriesData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ACCESS_MEMORIES;
    public override string name { get { return "Access Memories"; } }
    public override string description { get { return "Access the memories of a character."; } }
}