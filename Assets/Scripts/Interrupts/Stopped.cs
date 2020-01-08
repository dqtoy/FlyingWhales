﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Stopped : Interrupt {
        public Stopped() : base(INTERRUPT.Stopped) {
            duration = 0;
            doesStopCurrentAction = true;
            doesDropCurrentJob = true;
        }

        //#region Overrides
        //public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
        //    actor.traitContainer.AddTrait(actor, "Resting");
        //    return true;
        //}
        //public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
        //    actor.traitContainer.RemoveTrait(actor, "Resting");
        //    return true;
        //}
        //#endregion
    }
}