using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class RevertToNormal : Interrupt {
        public RevertToNormal() : base(INTERRUPT.Revert_To_Normal) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.lycanData.RevertToNormal();
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        #endregion
    }
}
