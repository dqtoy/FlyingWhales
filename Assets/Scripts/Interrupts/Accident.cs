using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Accident : Interrupt {
        public Accident() : base(INTERRUPT.Accident) {
            duration = 1;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            if(actor.traitContainer.AddTrait(actor, "Injured")) {
                return true;
            }
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        #endregion
    }
}

