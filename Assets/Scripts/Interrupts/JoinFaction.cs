using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class JoinFaction : Interrupt {
        public JoinFaction() : base(INTERRUPT.Join_Faction) {
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