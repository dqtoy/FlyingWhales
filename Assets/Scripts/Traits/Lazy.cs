using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lazy : Trait {

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

            if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
            }

            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = character });
            character.jobQueue.AddJobInQueue(job);
        }
    }
    #endregion
}
