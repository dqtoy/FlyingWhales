﻿using System.Collections;
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
        UIManager.Instance.characterInfoUI.SetLogMenuState(true);
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
