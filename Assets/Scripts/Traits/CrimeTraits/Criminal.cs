using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Interrupts;

namespace Traits {
    public class Criminal : Trait {

        public CrimeData crimeData { get; protected set; }
        public Character owner { get; private set; }

        public Criminal() {
            name = "Criminal";
            description = "This character has been branded as a criminal by his/her own faction.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                //When a character gains this Trait, add this log to the location and the character:
                //Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "add_criminal");
                //addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                //sourceCharacter.AddHistory(addLog);
                //TODO: sourceCharacter.homeSettlement.jobQueue.UnassignAllJobsTakenBy(sourceCharacter);
                owner.CancelOrUnassignRemoveTraitRelatedJobs();
            }

        }
        //public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        //    if (sourcePOI is Character) {
        //        Character sourceCharacter = sourcePOI as Character;
        //        //When a character loses this Trait, add this log to the location and the character:
        //        Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "remove_criminal");
        //        addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //        sourceCharacter.AddHistory(addLog);
        //    }
        //    base.OnRemoveTrait(sourcePOI, removedBy);
        //}
        /// <summary>
        /// Make this character create an apprehend job at his home location targetting a specific character.
        /// </summary>
        /// <param name="targetCharacter">The character to be apprehended.</param>
        /// <returns>The created job.</returns>
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                //TODO: (gainedFromDoing == null || gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob)) &&
                if (targetCharacter.isAtHomeRegion && characterThatWillDoJob.isAtHomeRegion
                    && targetCharacter.homeRegion == characterThatWillDoJob.homeRegion
                    && characterThatWillDoJob.homeSettlement != null
                    && !targetCharacter.isDead) { // && targetCharacter.traitContainer.GetNormalTrait<Trait>("Restrained") == null
                    GoapPlanJob currentJob = targetCharacter.GetJobTargettingThisCharacter(JOB_TYPE.APPREHEND);
                    if (currentJob == null) {
                        if (InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter)) {
                            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP, targetCharacter, characterThatWillDoJob);
                            //job.SetCanBeDoneInLocation(true);
                            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { characterThatWillDoJob.homeSettlement.prison });
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
        public override string GetNameInUI(ITraitable traitable) {
            if(crimeData != null) {
                return name + ":" + crimeData.crime.name;
            }
            return name;
        }
        #endregion

        #region General
        public void SetCrime(CRIME_TYPE crimeType, IReactable crime) {
            if(crimeData != null) {
                Debug.LogError("Cannot set crime to criminal " + owner.name + " because it already has a crime: " + crimeData.crimeType.ToString());
                return;
            }
            crimeData = new CrimeData(crimeType, crime, owner);
        }
        #endregion
    }
}

