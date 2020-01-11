using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Injured : Trait {
        private Character _sourceCharacter;
        //private GoapPlanJob _removeTraitJob;

        //#region getters/setters
        //public override bool isRemovedOnSwitchAlterEgo {
        //    get { return true; }
        //}
        //#endregion

        public Injured() {
            name = "Injured";
            description = "This character is badly hurt.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FIRST_AID_CHARACTER, };
            moodEffect = -4;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable traitable) {
            base.OnAddTrait(traitable);
            if (traitable is Character) {
                _sourceCharacter = traitable as Character;
                _sourceCharacter.UpdateCanCombatState();
                _sourceCharacter.AdjustSpeedModifier(-0.15f);
                //_sourceCharacter.CreateRemoveTraitJob(name);
                _sourceCharacter.AddTraitNeededToBeRemoved(this);
                if (gainedFromDoing == null) { //TODO: || gainedFromDoing.poiTarget != _sourceCharacter
                    _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "add_trait", null, name.ToLower());
                } else {
                    if (gainedFromDoing.goapType == INTERACTION_TYPE.ASSAULT) {
                        Log addLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "add_trait", gainedFromDoing);
                        addLog.AddToFillers(_sourceCharacter, _sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        addLog.AddToFillers(this, this.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        //TODO: gainedFromDoing.states["Target Injured"].AddArrangedLog("injured", addLog, () => PlayerManager.Instance.player.ShowNotificationFrom(addLog, _sourceCharacter, true));
                    }
                }
                //Messenger.Broadcast(Signals.TRANSFER_ENGAGE_TO_FLEE_LIST, _sourceCharacter);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            _sourceCharacter.UpdateCanCombatState();
            _sourceCharacter.AdjustSpeedModifier(0.15f);
            //if (_removeTraitJob != null) {
            //    _removeTraitJob.jobQueueParent.CancelJob(_removeTraitJob);
            //}
            _sourceCharacter.ForceCancelAllJobsTargettingThisCharacterExcept(JOB_TYPE.REMOVE_TRAIT, name, removedBy);
            _sourceCharacter.RemoveTraitNeededToBeRemoved(this);
            _sourceCharacter.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "remove_trait", null, name.ToLower());
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead && !targetCharacter.traitContainer.HasTraitOf(TRAIT_TYPE.CRIMINAL)) {
                    SerialKiller serialKiller = characterThatWillDoJob.traitContainer.GetNormalTrait<Trait>("Serial Killer") as SerialKiller;
                    if (serialKiller != null) {
                        serialKiller.SerialKillerSawButWillNotAssist(targetCharacter, this);
                        return false;
                    }
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.REMOVE_TRAIT, name);
                    if (currentJob == null) {
                        if (!IsResponsibleForTrait(characterThatWillDoJob) && InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter)) {
                            GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = name, target = GOAP_EFFECT_TARGET.TARGET };
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REMOVE_TRAIT, goapEffect, targetCharacter, characterThatWillDoJob);
                            job.AddOtherData(INTERACTION_TYPE.CRAFT_ITEM, new object[] { SPECIAL_TOKEN.HEALING_POTION });
                            job.AddOtherData(INTERACTION_TYPE.TAKE_RESOURCE, new object[] { TokenManager.Instance.itemData[SPECIAL_TOKEN.HEALING_POTION].craftCost });
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                    } 
                    //else {
                    //    if (InteractionManager.Instance.CanCharacterTakeRemoveIllnessesJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                    //        return TryTransferJob(currentJob, characterThatWillDoJob);
                    //    }
                    //}
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }

}
