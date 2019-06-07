using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abduct : PlayerJobAction {

    private Character _targetCharacter;
    private List<Area> _abductAreas;
    public Abduct() {
        name = "Abduct";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        _abductAreas = new List<Area>();
        for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
            Area area = LandmarkManager.Instance.allAreas[i];
            if (area.name == "Cardell" || area.name == "Denrio") {
                _abductAreas.Add(area);
            }
        }
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        _targetCharacter = targetPOI as Character;
        string titleText = "Select a location.";
        UIManager.Instance.ShowClickableObjectPicker(_abductAreas, OnClickArea, null, CanClickArea, titleText);
    }

    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead || character.id == targetCharacter.id) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
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
        if (targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }

    #region Area Checkers
    private void OnClickArea(Area area) {
        UIManager.Instance.ShowClickableObjectPicker(area.charactersAtLocation, RileUpCharacter, null, CanRileUpCharacter, "Choose a character to abduct.");
    }
    private bool CanClickArea(Area area) {
        if (PlayerManager.Instance.player.playerArea == area) {
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
    private bool CanRileUpCharacter(Character character) {
        if (_targetCharacter == character) {
            return false;
        }
        if (_targetCharacter.faction == character.faction) { //_targetCharacter.faction != FactionManager.Instance.neutralFaction &&
            return false;
        }
        if (!_targetCharacter.IsAvailable()) {
            return false;
        }
        return true;
    }
    #endregion
}
