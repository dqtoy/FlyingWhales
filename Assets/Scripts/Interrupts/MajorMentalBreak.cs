using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class MajorMentalBreak : Interrupt {
        public MajorMentalBreak() : base(INTERRUPT.Major_Mental_Break) {
            duration = 0;
            isSimulateneous = true;
        }

        //#region Overrides
        //public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
        //    Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Major Mental Break", "major_break");
        //    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //    return true;
        //}
        //#endregion
    }
}
