using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingBrokenhearted : Interrupt {
        public FeelingBrokenhearted() : base(INTERRUPT.Feeling_Brokenhearted) {
            duration = 12;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY);
            return true;
        }
        #endregion
    }
}
