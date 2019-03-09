using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RileUp : PlayerJobAction {

    private Character _targetCharacter;

    public RileUp() {
        actionName = "Rile Up";
        SetDefaultCooldownTime(3);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, Character targetCharacter) {
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
            return;
        }
        _targetCharacter = targetCharacter;
        UIManager.Instance.ShowClickableObjectPicker(LandmarkManager.Instance.allAreas, OnClickArea, null, CanClickArea);
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id || (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)) {
            return false;
        }
        if(targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }

    #region Area Checkers
    private void OnClickArea(Area area) {
        UIManager.Instance.ShowClickableObjectPicker(area.charactersAtLocation, RileUpCharacter, null, CanRileUpCharacter);
    }
    private bool CanClickArea(Area area) {
        if(PlayerManager.Instance.player.playerArea == area) {
            return false;
        }
        return true;
    }
    #endregion

    #region Character Checkers
    private void RileUpCharacter(Character character) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();

        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = _targetCharacter.homeArea, targetPOI = character };
        _targetCharacter.StartGOAP(goapEffect, character);
    }
    private bool CanRileUpCharacter(Character character) {
        if(_targetCharacter == character) {
            return false;
        }
        if(_targetCharacter.faction != FactionManager.Instance.neutralFaction && _targetCharacter.faction == character.faction) {
            return false;
        }
        return true;
    }
    #endregion
}
