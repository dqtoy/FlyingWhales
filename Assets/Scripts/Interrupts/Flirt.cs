using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Flirt : Interrupt {
        public Flirt() : base(INTERRUPT.Flirt) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.nonActionEventsComponent.NormalFlirtCharacter(target as Character);
            return true;
        }
        #endregion
    }
}