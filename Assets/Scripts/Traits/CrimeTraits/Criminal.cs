using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public abstract class Criminal : Trait {

        public CRIME_CATEGORY crimeSeverity { get; protected set; }

        public Criminal() {
            name = "Criminal";
            description = "This character has been branded as a criminal by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character sourceCharacter = sourcePOI as Character;
                //When a character gains this Trait, add this log to the location and the character:
                Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "add_criminal");
                addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                sourceCharacter.AddHistory(addLog);
                //TODO: sourceCharacter.homeArea.jobQueue.UnassignAllJobsTakenBy(sourceCharacter);
                sourceCharacter.CancelOrUnassignRemoveTraitRelatedJobs();
            }

        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (sourcePOI is Character) {
                Character sourceCharacter = sourcePOI as Character;
                //When a character loses this Trait, add this log to the location and the character:
                Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "remove_criminal");
                addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                sourceCharacter.AddHistory(addLog);
            }
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        /// <summary>
        /// Make this character create an apprehend job at his home location targetting a specific character.
        /// </summary>
        /// <param name="targetCharacter">The character to be apprehended.</param>
        /// <returns>The created job.</returns>
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                //TODO: (gainedFromDoing == null || gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob)) &&
                if (targetCharacter.isAtHomeRegion && !targetCharacter.isDead && targetCharacter.traitContainer.GetNormalTrait("Restrained") == null) {
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.APPREHEND);
                    if (currentJob == null) {
                        if (InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter)) {
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, characterThatWillDoJob);
                            //job.SetCanBeDoneInLocation(true);
                            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.specificLocation.prison });
                            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                            return true;
                        }
                    } 
                    //else {
                    //    if (InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, currentJob)) {
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

