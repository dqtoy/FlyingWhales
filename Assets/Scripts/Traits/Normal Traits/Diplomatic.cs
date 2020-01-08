using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Diplomatic : Trait {
        public Diplomatic() {
            name = "Diplomatic";
            description = "Diplomatic characters do not have enemies and may improve relationship of other characters.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            ticksDuration = 0;
        }

        #region Overrides
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 20) {
                    if ((targetCharacter.stateComponent.currentState == null || (targetCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT && targetCharacter.stateComponent.currentState.characterState != CHARACTER_STATE.BERSERKED))
                        && targetCharacter.faction == characterThatWillDoJob.faction && targetCharacter.role.roleType != CHARACTER_ROLE.BEAST
                        && !targetCharacter.returnedToLife && !targetCharacter.doNotDisturb
                        && targetCharacter.opinionComponent.HasCharacterWithOpinionLabel(OpinionComponent.Enemy, OpinionComponent.Rival)) {
                        characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Reduce_Conflict, targetCharacter);
                        //if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.RESOLVE_CONFLICT)) {
                        //    GoapPlanJob resolveConflictJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.RESOLVE_CONFLICT, INTERACTION_TYPE.RESOLVE_CONFLICT, targetCharacter, characterThatWillDoJob);
                        //    characterThatWillDoJob.jobQueue.AddJobInQueue(resolveConflictJob);
                        //}

                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

