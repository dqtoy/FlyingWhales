using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class TransformToWolf : Interrupt {
        public TransformToWolf() : base(INTERRUPT.Transform_To_Wolf) {
            duration = 6;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.lycanData.TurnToWolf();
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        #endregion
    }
}
