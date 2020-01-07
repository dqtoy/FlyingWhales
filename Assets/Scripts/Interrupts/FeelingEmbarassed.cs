using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class FeelingEmbarassed : Interrupt {
        public FeelingEmbarassed() : base(INTERRUPT.Feeling_Embarassed) {
            duration = 0;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.marker.AddAvoidInRange(target, true, "embarassed");
            return true;
        }
        #endregion
    }
}