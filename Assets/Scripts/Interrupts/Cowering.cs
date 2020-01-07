using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Cowering : Interrupt {
        public Cowering() : base(INTERRUPT.Cowering) {
            duration = 12;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEffect(Character actor, IPointOfInterest target) {
            actor.CancelAllJobs();
            return true;
        }
        #endregion
    }
}