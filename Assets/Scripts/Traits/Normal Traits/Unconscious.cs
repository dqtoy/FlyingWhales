using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Unconscious : Trait {
        private Character _sourceCharacter;
        public override bool isRemovedOnSwitchAlterEgo {
            get { return true; }
        }

        public Unconscious() {
            name = "Unconscious";
            description = "This character is unconscious.";
            thoughtText = "[Character] is unconscious.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            daysDuration = 24; //144
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER };
            hindersMovement = true;
            hindersWitness = true;
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
            if (sourceCharacter is Character) {
                _sourceCharacter = sourceCharacter as Character;
                if (_sourceCharacter.currentHP <= 0) {
                    _sourceCharacter.SetHP(1);
                }
                //CheckToApplyRestrainJob();
                //_sourceCharacter.CreateRemoveTraitJob(name);
                _sourceCharacter.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoing == null) { //TODO: || gainedFromDoing.poiTarget != _sourceCharacter
                    _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait");
                    addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    //if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT_CHARACTER) {
                    //    gainedFromDoing.states["Target Knocked Out"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //} else if (gainedFromDoing.goapType == INTERACTION_TYPE.KNOCKOUT_CHARACTER) {
                    //    gainedFromDoing.states["Knockout Success"].AddArrangedLog("unconscious", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    //}
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
            //_sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.RESTRAIN, removedBy); //so that the character that restrained him will not cancel his job.
            _sourceCharacter.CancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy); //so that the character that cured him will not cancel his job.
            _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnDeath(Character character) {
            base.OnDeath(character);
            character.traitContainer.RemoveTrait(character, this);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead && targetCharacter.faction == characterThatWillDoJob.faction && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                    SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait("Serial Killer") as SerialKiller;
                    if (serialKiller != null) {
                        serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                        return false;
                    }
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
                    if (currentJob == null) {
                        GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter,
                            new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION } }, }, characterThatWillDoJob);
                        if (InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, job)) {
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                    } else {
                        if (InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                            TryTransferJob(currentJob, characterThatWillDoJob);
                        }
                    }
                }
                if (!targetCharacter.isDead && targetCharacter.faction != characterThatWillDoJob.faction && targetCharacter.traitContainer.GetNormalTrait("Restrained") == null) {
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.RESTRAIN);
                    if (currentJob == null) {
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, new Dictionary<INTERACTION_TYPE, object[]>() {
                            { INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.specificLocation.prison } }
                        }, characterThatWillDoJob);
                        //job.SetCanBeDoneInLocation(true);
                        if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, job)) {
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
  
                    } else {
                        if (InteractionManager.Instance.CanCharacterTakeRestrainJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                            TryTransferJob(currentJob, characterThatWillDoJob);
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }
}
