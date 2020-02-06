using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class MentalBreak : Interrupt {
        public MentalBreak() : base(INTERRUPT.Mental_Break) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Mental Break", "break");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, actor.moodComponent.mentalBreakName, LOG_IDENTIFIER.STRING_1);
            actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}
