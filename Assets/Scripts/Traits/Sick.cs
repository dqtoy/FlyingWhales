using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sick : Trait {
    private Character _sourceCharacter;
    //private GoapPlanJob _removeTraitJob;

    public Sick() {
        name = "Sick";
        description = "This character is sick.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 480;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        if (sourceCharacter is Character) {
            _sourceCharacter = sourceCharacter as Character;
            _sourceCharacter.AdjustSpeedModifier(-0.10f);
            //_sourceCharacter.CreateRemoveTraitJob(name);
            _sourceCharacter.AddTraitNeededToBeRemoved(this);
            if (gainedFromDoing == null) {
                _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
            } else {
                if (gainedFromDoing.goapType == INTERACTION_TYPE.EAT_DWELLING_TABLE) {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                    addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    gainedFromDoing.states["Eat Poisoned"].AddArrangedLog("sick", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                } else {
                    _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                }
            }
        }
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        _sourceCharacter.AdjustSpeedModifier(0.10f);
        //if (_removeTraitJob != null) {
        //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
        //}
        _sourceCharacter.CancelAllJobsTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
        _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
        _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
        base.OnRemoveTrait(sourceCharacter, removedBy);
        //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect,
                    new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, });
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
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
