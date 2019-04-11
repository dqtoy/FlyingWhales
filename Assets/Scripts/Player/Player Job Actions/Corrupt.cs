using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corrupt : PlayerJobAction {
    private Character _targetCharacter;

    private List<string> _traitNames;

    public Corrupt() {
        actionName = "Corrupt";
        SetDefaultCooldownTime(48);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        _traitNames = new List<string>() { "Lycanthropy" };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        _targetCharacter = targetCharacter;
        UIManager.Instance.ShowClickableObjectPicker(_traitNames, OnClickTrait, null, CanCorruptCharacter);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }

    #region Trait Checkers
    private void OnClickTrait(string traitName) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();
        if (traitName == "Lycanthropy") {
            Lycanthropy lycanthropy = new Lycanthropy();
            _targetCharacter.AddTrait(lycanthropy);
        }
    }
    private bool CanCorruptCharacter(string traitName) {
        if (_targetCharacter.GetTrait(traitName) != null) {
            return false;
        }
        return true;
    }
    #endregion
}
