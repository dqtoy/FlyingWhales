using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Murderer : Trait {
        public override bool isPersistent { get { return true; } }
        public Murderer() {
            name = "Murderer";
            description = "This character has been branded as a Murderer by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            daysDuration = 0;
            crimeSeverity = CRIME_CATEGORY.SERIOUS;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        /// <summary>
        /// Make this character create an apprehend job at his home location targetting a specific character.
        /// </summary>
        /// <param name="targetCharacter">The character to be apprehended.</param>
        /// <returns>The created job.</returns>
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if ((gainedFromDoing == null || gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob)) && targetCharacter.isAtHomeRegion && !targetCharacter.isDead
                   && targetCharacter.traitContainer.GetNormalTrait("Restrained") == null) {
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.APPREHEND);
                    if (currentJob == null) {
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.IMPRISON_CHARACTER, targetCharacter);
                        //job.SetCanBeDoneInLocation(true);
                        if (InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, job)) {
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                        //else {
                        //    job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeApprehendJob);
                        //    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                        //    return false;
                        //}
                    } else {
                        if (currentJob.jobQueueParent.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                            bool canBeTransfered = false;
                            if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentAction != null
                                && currentJob.assignedCharacter.currentAction.parentPlan != null && currentJob.assignedCharacter.currentAction.parentPlan.job == currentJob) {
                                if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                    canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentAction.poiTarget);
                                }
                            } else {
                                canBeTransfered = true;
                            }
                            if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                                currentJob.jobQueueParent.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                                characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                                characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                                return true;
                            }
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        #endregion
    }
}

