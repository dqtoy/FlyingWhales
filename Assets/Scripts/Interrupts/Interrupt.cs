using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Interrupt : IReactable {
        public INTERRUPT interrupt { get; protected set; }
        public string name { get; protected set; }
        public int duration { get; protected set; }
        public bool isSimulateneous { get; protected set; }
        public bool doesStopCurrentAction { get; protected set; }
        public bool doesDropCurrentJob { get; protected set; }

        public Interrupt(INTERRUPT interrupt) {
            this.interrupt = interrupt;
            this.name = Utilities.NotNormalizedConversionEnumToString(interrupt.ToString());
            isSimulateneous = false;
        }

        #region Virtuals
        public virtual bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) { return false; }
        public virtual bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) { return false; }
        #endregion
    }
}