using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeSettlementRuler : Interrupt {
        public BecomeSettlementRuler() : base(INTERRUPT.Become_Settlement_Ruler) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.homeSettlement.SetRuler(actor);
            return true;
        }
        #endregion
    }
}