using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Criminal : Trait {
    public override bool isPersistent { get { return true; } }
    public Criminal() {
        name = "Criminal";
        description = "This character has been branded as a criminal by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
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
            ////sourceCharacter.specificLocation.AddHistory(addLog);
        }
        
    }
    public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            //When a character loses this Trait, add this log to the location and the character:
            Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "remove_criminal");
            addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            sourceCharacter.AddHistory(addLog);
            //sourceCharacter.specificLocation.AddHistory(addLog);
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
            if ((gainedFromDoing == null || gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob)) && targetCharacter.isAtHomeRegion && !targetCharacter.isDead
               && targetCharacter.GetNormalTrait("Restrained") == null) {
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
                    if (currentJob.currentOwner.isAreaOrQuestJobQueue && InteractionManager.Instance.CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, currentJob)) {
                        bool canBeTransfered = false;
                        if (currentJob.assignedCharacter != null && currentJob.assignedCharacter.currentActionNode != null
                            && currentJob.assignedCharacter.currentActionNode.parentPlan != null && currentJob.assignedCharacter.currentActionNode.parentPlan.job == currentJob) {
                            if (currentJob.assignedCharacter != characterThatWillDoJob) {
                                canBeTransfered = !currentJob.assignedCharacter.marker.inVisionPOIs.Contains(currentJob.assignedCharacter.currentActionNode.poiTarget);
                            }
                        } else {
                            canBeTransfered = true;
                        }
                        if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                            currentJob.currentOwner.CancelJob(currentJob, shouldDoAfterEffect: false, forceRemove: true);
                            characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob, false);
                            characterThatWillDoJob.jobQueue.CurrentTopPriorityIsPushedBackBy(currentJob, characterThatWillDoJob);
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
