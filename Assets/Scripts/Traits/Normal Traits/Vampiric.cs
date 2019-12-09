using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class Vampiric : Trait {
        //private Character _character;

        //private int _flatAttackMod;
        private int _flatHPMod;
        //private int _flatSpeedMod;

        public Vampiric() {
            name = "Vampiric";
            description = "Vampires drink other character's blood for sustenance.";
            thoughtText = "[Character] sucks blood.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            _flatHPMod = 500;
            canBeTriggered = true;
        }

        public void VamipiricLevel(int level) {
            //_flatAttackMod *= level;
            //_flatHPMod *= level;
            //_flatSpeedMod *= level;
            if (level == 1) {
                _flatHPMod *= 1;
            } else if (level == 2) {
                _flatHPMod = Mathf.RoundToInt(_flatHPMod * 1.5f);
            } else if (level == 3) {
                _flatHPMod *= 2;
            }
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING, JOB_TYPE.TIREDNESS_RECOVERY, JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED);
                character.SetTirednessForcedTick(0);
                character.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LATE_NIGHT);
                character.SetFullnessForcedTick();
                character.AdjustDoNotGetTired(1);
                character.ResetTirednessMeter();
                //character.AdjustAttackMod(_flatAttackMod);
                character.AdjustMaxHPMod(_flatHPMod);
                //character.AdjustSpeedMod(_flatSpeedMod);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (sourceCharacter is Character) {
                Character character = sourceCharacter as Character;
                character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
                character.SetTirednessForcedTick();
                character.SetForcedFullnessRecoveryTimeInWords(TIME_IN_WORDS.LUNCH_TIME);
                character.SetFullnessForcedTick();
                character.AdjustDoNotGetTired(-1);
                //character.AdjustAttackMod(-_flatAttackMod);
                character.AdjustMaxHPMod(-_flatHPMod);
                //character.AdjustSpeedMod(-_flatSpeedMod);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            VamipiricLevel(level);
        }
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is Character) {
        //        //In Vampiric, the parameter traitOwner is the target character, that's why you must pass the target character in this parameter not the actual owner of the trait, the actual owner of the trait is the characterThatWillDoJob
        //        //Character targetCharacter = targetPOI as Character;
        //        //if (characterThatWillDoJob.currentActionNode.action != null && characterThatWillDoJob.currentActionNode.action.goapType == INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD && !characterThatWillDoJob.currentActionNode.isDone) {
        //        //    if (characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) != RELATIONSHIP_EFFECT.POSITIVE && targetCharacter.traitContainer.GetNormalTrait("Vampiric") == null && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
        //        //        //TODO: GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(characterThatWillDoJob.currentJobNode.jobType, INTERACTION_TYPE.DRINK_BLOOD, targetCharacter);
        //        //        //job.SetIsStealth(true);
        //        //        //characterThatWillDoJob.currentActionNode.action.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentActionNode.action.parentPlan.job);
        //        //        //characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
        //        //        //characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
        //        //        return true;
        //        //    }
        //        //}
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        public override string TriggerFlaw(Character character) {
            //The character will begin Hunt for Blood.
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                if (character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                    character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
                }
                bool triggerGrieving = false;
                Griefstricken griefstricken = character.traitContainer.GetNormalTrait("Griefstricken") as Griefstricken;
                if (griefstricken != null) {
                    triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerGrieving) {
                    GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR), character, character);
                    character.jobQueue.AddJobInQueue(job);
                } else {
                    griefstricken.TriggerGrieving();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref List<GoapEffect> effects) {
            if (action == INTERACTION_TYPE.DRINK_BLOOD) {
                effects.Add(new GoapEffect(GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
            }
        }
        #endregion
    }
}

