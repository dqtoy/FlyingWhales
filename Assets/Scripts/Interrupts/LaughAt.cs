using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LaughAt : Interrupt {
        public LaughAt() : base(INTERRUPT.Laugh_At) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptEndEffect(Character actor, IPointOfInterest target) {
            Character targetCharacter = target as Character;
            if (targetCharacter.canWitness) {
                targetCharacter.traitContainer.AddTrait(targetCharacter, "Ashamed");
                return true;
            }
            return base.ExecuteInterruptEndEffect(actor, target);
        }
        #endregion
    }
}
