using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeFactionLeader : Interrupt {
        public BecomeFactionLeader() : base(INTERRUPT.Become_Faction_Leader) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.faction.SetLeader(actor);
            return true;
        }
        #endregion
    }
}
