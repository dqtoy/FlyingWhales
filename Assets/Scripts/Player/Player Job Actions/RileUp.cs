using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RileUp : PlayerJobAction {

    private Character _targetCharacter;
    private List<Area> _abductAreas;
    public RileUp() {
        name = "Rile Up";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        _abductAreas = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area area = LandmarkManager.Instance.allAreas[i];
            if(area.name == "Cardell" || area.name == "Denrio") {
                _abductAreas.Add(area);
            }
        }
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        //if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
        //    return;
        //}
        if (!(targetPOI is Character)) {
            return;
        }
        _targetCharacter = targetPOI as Character;
        string titleText = "Select a location.";
        if(_targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
            titleText = "Select a location and " + _targetCharacter.name + " will run amok there.";
            UIManager.Instance.ShowClickableObjectPicker(LandmarkManager.Instance.allAreas, OnClickArea, null, CanClickArea, titleText);
        } else {
            UIManager.Instance.ShowClickableObjectPicker(_abductAreas, OnClickArea, null, CanClickArea, titleText);
        }
    }

    protected override bool ShouldButtonBeInteractable(Character character, Character targetCharacter) {
        name = GetActionName(targetCharacter);
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if(targetCharacter.role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        return base.ShouldButtonBeInteractable(character, targetCharacter);
    }

    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead || assignedCharacter == targetCharacter) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType != CHARACTER_ROLE.BEAST && targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    public override string GetActionName(Character target) {
        if (target.role.roleType != CHARACTER_ROLE.BEAST) {
            return "Abduct";
        } else {
            return "Rile Up";
        }
    }

    #region Area Checkers
    private void OnClickArea(Area area) {
        if(_targetCharacter.role.roleType == CHARACTER_ROLE.BEAST) {
            RileUpCharacter(area);
        } else {
            UIManager.Instance.ShowClickableObjectPicker(area.charactersAtLocation, RileUpCharacter, null, CanRileUpCharacter, "Choose a character to abduct.");
        }
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
        GoapPlanJob job = new GoapPlanJob("Abduct", goapEffect);
        job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = character }, INTERACTION_TYPE.ABDUCT_ACTION);
        job.SetCannotOverrideJob(true);
        job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
        _targetCharacter.jobQueue.AddJobInQueue(job, true, false);
        _targetCharacter.jobQueue.ProcessFirstJobInQueue(_targetCharacter);
        //_targetCharacter.StartGOAP(goapEffect, character, GOAP_CATEGORY.REACTION);
    }
    private void RileUpCharacter(Area area) {
        base.ActivateAction(assignedCharacter, _targetCharacter);
        UIManager.Instance.HideObjectPicker();

        _targetCharacter.AdjustIsWaitingForInteraction(1);
        if (_targetCharacter.currentAction != null) {
            _targetCharacter.currentAction.StopAction();
        }
        _targetCharacter.DropAllPlans();
        _targetCharacter.AdjustIsWaitingForInteraction(-1);

        if(_targetCharacter.stateComponent.currentState != null) {
            _targetCharacter.stateComponent.currentState.OnExitThisState();
        }
        //This is double in case the character is in a minor state and has previous major state, so that the previous major state will end too
        if (_targetCharacter.stateComponent.currentState != null) {
            _targetCharacter.stateComponent.currentState.OnExitThisState();
        }

        CharacterStateJob job = new CharacterStateJob("Berserked", CHARACTER_STATE.BERSERKED, area);
        _targetCharacter.jobQueue.AddJobInQueue(job, true);
        //_targetCharacter.AddTrait("Berserker");
        //_targetCharacter.currentParty.GoToLocation(area, PATHFINDING_MODE.NORMAL);
    }
    private bool CanRileUpCharacter(Character character) {
        if(_targetCharacter == character) {
            return false;
        }
        if(_targetCharacter.faction == character.faction) { //_targetCharacter.faction != FactionManager.Instance.neutralFaction &&
            return false;
        }
        if(!_targetCharacter.IsAvailable()) {
            return false;
        }
        return true;
    }
    #endregion
}
