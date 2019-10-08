using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazy : Trait {
    public Character owner { get; private set; }

    public Lazy() {
        name = "Lazy";
        description = "Lazy characters often daydream and are less likely to take on settlement tasks.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;
        mutuallyExclusive = new string[] { "Hardworking" };
    }

    #region Overrides
    public override void OnAddTrait(ITraitable addedTo) {
        base.OnAddTrait(addedTo);
        if(addedTo is Character) {
            owner = addedTo as Character;
        }
    }
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //Will drop current action and will perform Happiness Recovery.
        if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
            if (character.currentAction != null) {
                character.StopCurrentAction(false);
            }
            if (character.stateComponent.currentState != null) {
                character.stateComponent.currentState.OnExitThisState();
            } else if (character.stateComponent.stateToDo != null) {
                character.stateComponent.SetStateToDo(null, false, false);
            }

            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = character.GetNormalTrait("Heartbroken") as Heartbroken;
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerBrokenhearted) {
                if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                    character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
                }
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = character });
                character.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
        }
    }
    #endregion

    public bool TriggerLazy() {
        if (!owner.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
            JOB_TYPE jobType = JOB_TYPE.HAPPINESS_RECOVERY;
            if (owner.isForlorn) {
                jobType = JOB_TYPE.HAPPINESS_RECOVERY_FORLORN;
            }
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = owner.GetNormalTrait("Heartbroken") as Heartbroken;
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = new GoapPlanJob(jobType, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = owner });
                job.SetCancelOnFail(true);
                owner.jobQueue.AddJobInQueue(job);

                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "trigger_lazy");
                log.AddToFillers(owner, owner.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                owner.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
            return true;
        }
        return false;
    }
}
