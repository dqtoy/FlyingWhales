using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
        //_flatAttackMod = 100;
        _flatHPMod = 500;
        //_flatSpeedMod = 100;
        canBeTriggered = true;
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    public void VamipiricLevel(int level) {
        //_flatAttackMod *= level;
        //_flatHPMod *= level;
        //_flatSpeedMod *= level;
        if(level == 1) {
            _flatHPMod *= 1;
        }else if (level == 2) {
            _flatHPMod = Mathf.RoundToInt(_flatHPMod * 1.5f);
        }else if (level == 3) {
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
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if(targetPOI is Character) {
            //In Vampiric, the parameter traitOwner is the target character, that's why you must pass the target character in this parameter not the actual owner of the trait, the actual owner of the trait is the characterThatWillDoJob
            Character targetCharacter = targetPOI as Character;
            if (characterThatWillDoJob.currentAction != null && characterThatWillDoJob.currentAction.goapType == INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD && !characterThatWillDoJob.currentAction.isDone) {
                if (characterThatWillDoJob.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.POSITIVE && characterThatWillDoJob.marker.CanDoStealthActionToTarget(targetCharacter)) {
                    GoapPlanJob job = new GoapPlanJob(characterThatWillDoJob.currentAction.parentPlan.job.jobType, INTERACTION_TYPE.DRINK_BLOOD, targetCharacter);
                    job.SetIsStealth(true);
                    characterThatWillDoJob.currentAction.parentPlan.job.jobQueueParent.CancelJob(characterThatWillDoJob.currentAction.parentPlan.job);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(job, false);
                    characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(job, characterThatWillDoJob);
                    return true;
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //The character will begin Hunt for Blood.
        if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
            if (character.jobQueue.HasJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING)) {
                character.jobQueue.CancelAllJobs(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING);
            }
            bool triggerGrieving = false;
            Griefstricken griefstricken = character.GetNormalTrait("Griefstricken") as Griefstricken;
            if (griefstricken != null) {
                triggerGrieving = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerGrieving) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = character });
                job.AddForcedInteraction(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = character }, INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD);
                job.SetCancelOnFail(true);
                character.jobQueue.AddJobInQueue(job);
            } else {
                griefstricken.TriggerGrieving();
            }
        }
    }
    #endregion
}
