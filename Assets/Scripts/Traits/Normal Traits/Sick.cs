using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Sick : Trait {
        private Character owner;
        private float pukeChance;
        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}
        public Sick() {
            name = "Sick";
            description = "This character has caught a mild illness.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            
            
            ticksDuration = 480;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CURE_CHARACTER, };
            mutuallyExclusive = new string[] { "Robust" };
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
                owner.AdjustSpeedModifier(-0.10f);
                //_sourceCharacter.CreateRemoveTraitJob(name);
                owner.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoing == null) {
                    owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    if (gainedFromDoing.goapType == INTERACTION_TYPE.EAT) {
                        Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                        addLog.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //TODO: gainedFromDoing.states["Eat Poisoned"].AddArrangedLog("sick", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, owner, true));
                    } else {
                        owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                    }
                }
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            owner.AdjustSpeedModifier(0.10f);
            //if (_removeTraitJob != null) {
            //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
            //}
            owner.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
            owner.RemoveTraitNeededToBeRemoved(this);
            owner.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
            base.OnRemoveTrait(sourceCharacter, removedBy);
            //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name) && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)
                    && !IsResponsibleForTrait(characterThatWillDoJob)) {
                    if (InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob(characterThatWillDoJob, targetCharacter)) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    } else {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob.currentRegion.area);
                        job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                        job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRemoveSpecialIllnessesJob);
                        characterThatWillDoJob.currentRegion.area.AddToAvailableJobs(job);
                    }
                    return true;
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        protected override void OnChangeLevel() {
            if (level == 1) {
                pukeChance = 5f;
            } else if (level == 2) {
                pukeChance = 7f;
            } else {
                pukeChance = 9f;
            }
        }
        public override bool PerTickOwnerMovement() {
            float pukeRoll = Random.Range(0f, 100f);
            bool hasCreatedJob = false;
            if (pukeRoll < pukeChance) {
                //do puke action
                if (owner.characterClass.className == "Zombie" || (owner.currentActionNode != null && owner.currentActionNode.action.goapType == INTERACTION_TYPE.PUKE)) {
                    return hasCreatedJob;
                }
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DEATH, INTERACTION_TYPE.PUKE, owner, owner);
                owner.jobQueue.AddJobInQueue(job);
                hasCreatedJob = true;
            }
            return hasCreatedJob;
        }
        #endregion
    }
}

