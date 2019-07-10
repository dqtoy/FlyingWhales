using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RileUp : PlayerJobAction {

    private Character _targetCharacter;
    public RileUp() : base(INTERVENTION_ABILITY.RILE_UP) {
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        _targetCharacter = targetPOI as Character;
        string titleText = "Select a location and " + _targetCharacter.name + " will run amok there.";
        UIManager.Instance.ShowClickableObjectPicker(LandmarkManager.Instance.allAreas, OnClickArea, null, CanClickArea, titleText);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        name = GetActionName(targetCharacter);
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if(targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }

    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead || assignedCharacter == targetCharacter) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType != CHARACTER_ROLE.BEAST) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    #region Area Checkers
    private void OnClickArea(Area area) {
        RileUpCharacter(area);
    }
    private bool CanClickArea(Area area) {
        if(PlayerManager.Instance.player.playerArea == area) {
            return false;
        }
        return true;
    }
    #endregion

    #region Character Checkers
    private void RileUpCharacter(Area area) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();

        //_targetCharacter.AdjustIsWaitingForInteraction(1);
        //if (_targetCharacter.currentAction != null) {
        //    _targetCharacter.currentAction.StopAction();
        //}
        //_targetCharacter.DropAllPlans();
        //_targetCharacter.AdjustIsWaitingForInteraction(-1);

        //if(_targetCharacter.stateComponent.currentState != null) {
        //    _targetCharacter.stateComponent.currentState.OnExitThisState();
        //}
        ////This is double in case the character is in a minor state and has previous major state, so that the previous major state will end too
        //if (_targetCharacter.stateComponent.currentState != null) {
        //    _targetCharacter.stateComponent.currentState.OnExitThisState();
        //}

        CharacterStateJob job = new CharacterStateJob(JOB_TYPE.BERSERK, CHARACTER_STATE.BERSERKED, area);
        _targetCharacter.jobQueue.AddJobInQueue(job);
        //_targetCharacter.AddTrait("Berserker");
        //_targetCharacter.currentParty.GoToLocation(area, PATHFINDING_MODE.NORMAL);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_rile_up");
        log.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(area, area.name, LOG_IDENTIFIER.LANDMARK_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }
    #endregion
}
