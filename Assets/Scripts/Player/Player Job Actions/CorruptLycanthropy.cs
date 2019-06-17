using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptLycanthropy : PlayerJobAction {
    private Character _targetCharacter;

    private List<string> _traitNames;

    public CorruptLycanthropy() {
        name = "Inflict Lycanthropy";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            _targetCharacter = targetPOI as Character;
        } else {
            return;
        }
        base.ActivateAction(assignedCharacter, _targetCharacter);
        Trait newTrait = new Lycanthropy();
        _targetCharacter.AddTrait(newTrait);
        _targetCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, newTrait.name);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) {
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Lycanthropy") != null) {
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
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Lycanthropy") != null) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
