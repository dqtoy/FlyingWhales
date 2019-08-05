using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dead : Trait {
    public Dead() {
        name = "Dead";
        description = "This character is dead.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.EAT_CORPSE };
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        if (traitOwner is Character) {
            Character targetCharacter = traitOwner as Character;
            if (targetCharacter.isDead && targetCharacter.race != RACE.SKELETON && !targetCharacter.HasJobTargettingThisCharacter(JOB_TYPE.BURY)) {
                GoapPlanJob buryJob = new GoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter);
                buryJob.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CORPSE);
                buryJob.AllowDeadTargets();
                if (CanTakeBuryJob(characterThatWillDoJob, null)) {
                    characterThatWillDoJob.jobQueue.AddJobInQueue(buryJob, false);
                    return true;
                } else {
                    buryJob.SetCanTakeThisJobChecker(CanTakeBuryJob);
                    characterThatWillDoJob.specificLocation.jobQueue.AddJobInQueue(buryJob, false);
                    return false;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    public override string GetToolTipText() {
        if (responsibleCharacter == null) {
            return description;
        }
        return "This character was killed by " + responsibleCharacter.name;
    }
    #endregion
}
