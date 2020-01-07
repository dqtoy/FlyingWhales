using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class Chat : Interrupt {
        public Chat() : base(INTERRUPT.Chat) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            actor.nonActionEventsComponent.ForceChatCharacter(target as Character);
            return true;
        }
        #endregion
    }
}