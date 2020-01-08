using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class NarcolepticAttack : Interrupt {
        public NarcolepticAttack() : base(INTERRUPT.Narcoleptic_Attack) {
            duration = 6;
            doesStopCurrentAction = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.traitContainer.AddTrait(actor, "Resting");
            return true;
        }
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.traitContainer.RemoveTrait(actor, "Resting");
            return true;
        }
        #endregion
    }
}