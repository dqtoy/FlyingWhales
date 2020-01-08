using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LeaveFaction : Interrupt {
        public LeaveFaction() : base(INTERRUPT.Leave_Faction) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        //public override bool ExecuteInterruptEffect(Character actor, IPointOfInterest target) {
        //    actor.nonActionEventsComponent.NormalFlirtCharacter(target as Character);
        //    return true;
        //}
        #endregion
    }
}