using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diplomatic : Trait {
    public Diplomatic() {
        name = "Diplomatic";
        description = "This character is peaceful.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is Character) {
            Character targetCharacter = targetPOI as Character;
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                List<Character> enemyCharactersOftarget = targetCharacter.GetCharactersWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
                if(enemyCharactersOftarget.Count > 1 || (enemyCharactersOftarget.Count == 1 && enemyCharactersOftarget[0] != characterThatWillDoJob)) {
                    GoapPlanJob resolveConflictJob = new GoapPlanJob(JOB_TYPE.RESOLVE_CONFLICT, INTERACTION_TYPE.RESOLVE_CONFLICT, targetCharacter);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(resolveConflictJob);
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}
