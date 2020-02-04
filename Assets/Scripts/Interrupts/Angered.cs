using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Angered : Interrupt {
        public Angered() : base(INTERRUPT.Angered) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.traitContainer.AddTrait(actor, "Angry");
            return true;
        }
        #endregion
    }
}
