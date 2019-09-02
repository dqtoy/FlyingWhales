﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thief : Trait {

    public override bool isPersistent { get { return true; } }

    public Thief() {
        name = "Thief";
        description = "This character has been branded as a Thief by his/her own faction.";
        type = TRAIT_TYPE.CRIMINAL;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
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
            if ((gainedFromDoing == null || gainedFromDoing.awareCharactersOfThisAction.Contains(characterThatWillDoJob)) && targetCharacter.isAtHomeArea && !targetCharacter.isDead && !targetCharacter.HasJobTargettingThis(JOB_TYPE.APPREHEND)
                && targetCharacter.GetNormalTrait("Restrained") == null) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.APPREHEND, INTERACTION_TYPE.DROP_CHARACTER, targetCharacter);
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Restrained", targetPOI = targetCharacter }, INTERACTION_TYPE.RESTRAIN_CHARACTER);
                job.SetCanBeDoneInLocation(true);
                if (CanCharacterTakeApprehendJob(characterThatWillDoJob, targetCharacter, null)) {
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
