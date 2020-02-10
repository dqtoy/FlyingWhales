﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Inspired : Interrupt {
        public Inspired() : base(INTERRUPT.Inspired) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target, ref Log overrideEffectLog) {
            actor.needsComponent.AdjustHope(5f);
            return true;
        }
        #endregion
    }
}
