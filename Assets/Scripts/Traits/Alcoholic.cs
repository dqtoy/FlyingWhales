using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alcoholic : Trait {

    public Alcoholic() {
        name = "Alcoholic";
        description = "Alcoholics enjoy drinking.";
        type = TRAIT_TYPE.FLAW;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        canBeTriggered = true;
    }

    #region Overrides
    public override void TriggerFlaw(Character character) {
        base.TriggerFlaw(character);
        //Will drink
        if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
            if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
            }
            GoapPlanJob drinkJob = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK);
            character.jobQueue.AddJobInQueue(drinkJob);
        }
    }
    #endregion
}
