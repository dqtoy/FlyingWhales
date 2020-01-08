using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BreakUp : Interrupt {
        public BreakUp() : base(INTERRUPT.Break_Up) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.nonActionEventsComponent.NormalBreakUp(target as Character);
            return true;
        }
        #endregion
    }
}