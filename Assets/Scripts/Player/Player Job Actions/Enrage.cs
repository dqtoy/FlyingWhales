using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enrage : PlayerJobAction {
    private Character _targetCharacter;

    public Enrage() {
        name = "Enrage";
        SetDefaultCooldownTime(3);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            _targetCharacter = targetPOI as Character;
        } else {
            return;
        }
        base.ActivateAction(assignedCharacter, _targetCharacter);
        _targetCharacter.stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, null, GameManager.Instance.GetTicksBasedOnMinutes(30));
        if (UIManager.Instance.characterInfoUI.isShowing) {
            UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
        }
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Enrage") != null) {
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
        if (targetCharacter.GetNormalTrait("Enrage") != null) {
            return false;
        }
        //if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter);
    }
}
