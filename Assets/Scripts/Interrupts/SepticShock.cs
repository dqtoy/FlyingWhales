using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class SepticShock : Interrupt {
        public SepticShock() : base(INTERRUPT.Septic_Shock) {
            duration = 4;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.Death("Septic Shock");
            return true;
        }
        #endregion
    }
}