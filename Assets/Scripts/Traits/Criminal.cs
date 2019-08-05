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
            sourceCharacter.specificLocation.AddHistory(addLog);
        }
        
    }
    public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
        if (sourcePOI is Character) {
            Character sourceCharacter = sourcePOI as Character;
            //When a character loses this Trait, add this log to the location and the character:
            Log addLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "remove_criminal");
            addLog.AddToFillers(sourceCharacter, sourceCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            sourceCharacter.AddHistory(addLog);
            sourceCharacter.specificLocation.AddHistory(addLog);
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
            if (gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob) && targetCharacter.isAtHomeArea && !targetCharacter.isDead && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.APPREHEND)
                && targetCharacter.GetNormalTrait("Restrained") == null) {
                GoapEffect goapEffect = new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = characterThatWillDoJob.homeArea, targetPOI = targetCharacter };
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.APPREHEND, goapEffect);
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                if (CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, null)) {
                    //job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    //job.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                    return true;
                } else {
                    job.SetCanTakeThisJobChecker(CanCharacterTakeApprehendJob);
                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(job);
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
