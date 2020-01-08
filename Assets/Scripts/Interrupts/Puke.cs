using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Puke : Interrupt {
        public Puke() : base(INTERRUPT.Puke) {
            duration = 2;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.SetPOIState(POI_STATE.INACTIVE);
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.SetPOIState(POI_STATE.ACTIVE);
            return true;
        }
        #endregion
    }
}