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
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourcePOI) {

    }
    public override void OnRemoveTrait(IPointOfInterest sourcePOI) {

    }
    public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
        Character targetCharacter = traitOwner as Character;
        if (targetCharacter.isDead && targetCharacter.race != RACE.SKELETON && !characterThatWillDoJob.HasTraitOf(TRAIT_TYPE.CRIMINAL) && characterThatWillDoJob.isAtHomeArea 
            && characterThatWillDoJob.role.roleType != CHARACTER_ROLE.BEAST) {
            //check first if the target character already has a bury job in this location
            GoapPlanJob buryJob = characterThatWillDoJob.jobQueue.GetJob(JOB_TYPE.BURY, targetCharacter) as GoapPlanJob;
            if (buryJob == null) {
                //if none, create one
                buryJob = new GoapPlanJob(JOB_TYPE.BURY, INTERACTION_TYPE.BURY_CHARACTER, targetCharacter);
                buryJob.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = targetCharacter }, INTERACTION_TYPE.CARRY_CORPSE);
                buryJob.SetCanTakeThisJobChecker(CanTakeBuryJob);
                buryJob.AllowDeadTargets();
                characterThatWillDoJob.jobQueue.AddJobInQueue(buryJob, false);
            }
            ////if the character is a soldier or civilian, and the bury job is currently unassigned, take the job
            //if (buryJob.assignedCharacter == null && (role.roleType == CHARACTER_ROLE.SOLDIER || role.roleType == CHARACTER_ROLE.CIVILIAN)) {
            //    //buryJob.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            //    homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
            //    //if (overrideCurrentAction) {
            //    //    buryJob.SetWillImmediatelyBeDoneAfterReceivingPlan(true);
            //    //    homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
            //    //} else {
            //    //    homeArea.jobQueue.AssignCharacterToJob(buryJob, this);
            //    //}
            //}
            return true;
        }
        return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
    }
    #endregion
}
