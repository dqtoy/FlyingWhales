using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abduct : PlayerSpell {

    private Character _targetCharacter;
    private List<Settlement> _abductAreas;

    public Abduct() : base(SPELL_TYPE.ABDUCT) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
        _abductAreas = new List<Settlement>();
        for (int i = 0; i < LandmarkManager.Instance.allSetttlements.Count; i++) {
            Settlement settlement = LandmarkManager.Instance.allSetttlements[i];
            if (settlement.name == "Cardell" || settlement.name == "Denrio") {
                _abductAreas.Add(settlement);
            }
        }
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        _targetCharacter = targetPOI as Character;
        //string titleText = "Select a location.";
        //UIManager.Instance.ShowClickableObjectPicker(_abductAreas, OnClickArea, null, CanClickArea, titleText);
    }

    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        //if (targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }

    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.race != RACE.SKELETON && targetCharacter.race != RACE.GOBLIN) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }

    #region Settlement Checkers
    private void OnClickArea(Settlement settlement) {
        //UIManager.Instance.ShowClickableObjectPicker(settlement.charactersAtLocation, RileUpCharacter, null, CanRileUpCharacter, "Choose a character to abduct.");
    }
    private bool CanClickArea(Settlement settlement) {
        if (PlayerManager.Instance.player.playerSettlement == settlement) {
            return false;
        }
        return true;
    }
    #endregion

    #region Character Checkers
    private void RileUpCharacter(Character character) {
        base.ActivateAction(_targetCharacter);
        UIManager.Instance.HideObjectPicker();

        ////GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = _targetCharacter.homeSettlement, targetPOI = character };
        //GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.ABDUCT, INTERACTION_TYPE.DROP_CHARACTER, character);
        ////job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = _targetCharacter, targetPOI = character }, INTERACTION_TYPE.CARRY_CHARACTER);
        //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = character }, INTERACTION_TYPE.ABDUCT_CHARACTER);
        ////job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = _targetCharacter.homeSettlement, targetPOI = character }, INTERACTION_TYPE.DROP_CHARACTER);
        //job.SetCannotOverrideJob(true);
        ////job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
        //_targetCharacter.jobQueue.AddJobInQueue(job);

        //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_abduct");
        //log.AddToFillers(_targetCharacter, _targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(character, character.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //log.AddLogToInvolvedObjects();
        //PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        ////_targetCharacter.jobQueue.ProcessFirstJobInQueue(_targetCharacter);
        ////_targetCharacter.StartGOAP(goapEffect, character, GOAP_CATEGORY.REACTION);
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

public class AbductData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.ABDUCT;
    public override string name { get { return "Abduct"; } }
    public override string description { get { return "Makes a character abduct other characters of different race."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }

    public AbductData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.CHARACTER };
    }
}
