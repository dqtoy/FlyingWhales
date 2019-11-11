using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Heartbroken : Trait {
        public Character owner { get; private set; }

        public Heartbroken() {
            name = "Heartbroken";
            description = "This character is heartbroken and may refuse entertainment.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(24);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if (sourcePOI is Character) {
                owner = sourcePOI as Character;
                owner.AdjustMoodValue(-25, this);
            }
            base.OnAddTrait(sourcePOI);
        }
        #endregion

        public GoapPlanJob TriggerBrokenhearted() {
            owner.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);

            GoapPlanJob job = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.FEELING_BROKENHEARTED, owner);
            job.SetCancelOnFail(true);
            owner.jobQueue.AddJobInQueue(job);
            return job;
        }
    }
}

