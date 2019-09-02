using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unconscious : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _restrainJob;
    //private GoapPlanJob _removeTraitJob;

    public Unconscious() {
        name = "Unconscious";
        description = "This character is unconscious.";
        thoughtText = "[Character] is unconscious.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 24; //144
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER }; //, INTERACTION_TYPE.DRINK_BLOOD
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override string GetToolTipText() {
        if (responsibleCharacter == null) {
            return description;
        }
        return "This character has been knocked out by " + responsibleCharacter.name;
    }
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if(sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            if (_sourceCharacter.currentHP <= 0) {
                _sourceCharacter.SetHP(1);
            }
            //CheckToApplyRestrainJob();
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.CancelAllJobsAndPlans();
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            if(gainedFromDoing == null || gainedFromDoing.poiTarget != _sourceCharacter) {
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_ACTION_NPC) {
                    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                }else if (gainedFromDoing.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                    gainedFromDoing.states["Knockout Success"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                }
            }
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        //if (_restrainJob != null) {
        //    _restrainJob.jobQueueParent.CancelJob(_restrainJob);
        //}
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.RESTRAIN, removedBy); //so that the character that restrained him will not cancel his job.
        _sourceCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && targetCharacter.faction == characterThatWillDoJob.faction && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
                job.SetCanBeDoneInLocation(true);
                if (CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, null)) {
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveTraitJob);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    if (!IsResponsibleForTrait(characterThatWillDoJob)) {
                        job.SetCanTakeThisJobChecker(CanCharacterTakeRemoveIllnessesJob);
                        characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    }
                    return false;
                }
            }
            if (!targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction && !targetCharacter.HasJobTargettingThis(JOB_TYPE.RESTRAIN) && targetCharacter.GetNormalTrait("Restrained") == null) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.RESTRAIN, INTERACTION_TYPE.DROP_CHARACTER, targetCharacter);
                //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, conditionKey = characterThatWillDoJob, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CHARACTER);
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                //job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = characterThatWillDoJob.specificLocation, targetPOI = targetCharacter }, INTERACTION_TYPE.DROP_CHARACTER);
                job.SetCanBeDoneInLocation(true);
                if (CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, null)) {
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                    //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    job.SetCanTakeThisJobChecker(CanCharacterTakeRestrainJob);
                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
