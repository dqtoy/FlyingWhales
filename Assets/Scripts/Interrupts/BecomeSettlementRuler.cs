using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class BecomeSettlementRuler : Interrupt {
        public BecomeSettlementRuler() : base(INTERRUPT.Become_Settlement_Ruler) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            actor.homeSettlement.SetRuler(actor);

            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Become Settlement Ruler", "became_ruler");
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(actor.homeSettlement, actor.homeSettlement.name, LOG_IDENTIFIER.LANDMARK_1);
            actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}