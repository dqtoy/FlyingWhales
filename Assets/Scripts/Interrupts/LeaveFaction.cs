using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class LeaveFaction : Interrupt {
        public LeaveFaction() : base(INTERRUPT.Leave_Faction) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            Faction prevFaction = actor.faction;
            if (actor.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction)) {
                Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Leave Faction", actor.interruptComponent.identifier);
                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                log.AddToFillers(prevFaction, prevFaction.name, LOG_IDENTIFIER.FACTION_1);
                actor.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                return true;
            }
            return base.ExecuteInterruptStartEffect(actor, target);
        }
        #endregion
    }
}