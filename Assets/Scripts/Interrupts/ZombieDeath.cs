using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class ZombieDeath : Interrupt {
        public ZombieDeath() : base(INTERRUPT.Zombie_Death) {
            duration = 0;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.Death("Zombie Virus");
            return true;
        }
        #endregion
    }
}