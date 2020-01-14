using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingConcerned : Interrupt {
        public FeelingConcerned() : base(INTERRUPT.Feeling_Concerned) {
            duration = 0;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.GO_TO, target, actor);
            return actor.jobQueue.AddJobInQueue(job);
        }
        #endregion
    }
}