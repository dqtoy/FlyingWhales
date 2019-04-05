using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corrupt : PlayerJobAction {
    private Character _targetCharacter;

    public Corrupt() {
        actionName = "Corrupt";
        SetDefaultCooldownTime(48);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        //_targetCharacter = targetCharacter;
        //UIManager.Instance.ShowClickableObjectPicker(LandmarkManager.Instance.allAreas, OnClickArea, null, CanClickArea);
        base.ActivateAction(assignedCharacter, targetCharacter);
        Lycanthropy lycanthropy = new Lycanthropy();
        targetCharacter.AddTrait(lycanthropy);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if(targetCharacter.GetTrait("Lycanthropy") != null) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }

    #region Area Checkers
    private void OnClickArea(Area area) {
        UIManager.Instance.ShowClickableObjectPicker(area.charactersAtLocation, RileUpCharacter, null, CanRileUpCharacter);
    }
    #endregion

    #region Character Checkers
    private void RileUpCharacter(Character character) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();

        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = _targetCharacter.homeArea, targetPOI = character };
        _targetCharacter.StartGOAP(goapEffect, character, GOAP_CATEGORY.REACTION);
    }
    private bool CanRileUpCharacter(Character character) {
        if (_targetCharacter == character) {
            return false;
        }
        if (_targetCharacter.faction != FactionManager.Instance.neutralFaction && _targetCharacter.faction == character.faction) {
            return false;
        }
        return true;
    }
    #endregion
}
