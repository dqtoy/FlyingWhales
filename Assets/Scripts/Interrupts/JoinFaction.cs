using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class JoinFaction : Interrupt {
        public JoinFaction() : base(INTERRUPT.Join_Faction) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            if(target is Character) {
                Character targetCharacter = target as Character;
                Faction factionToJoinTo = targetCharacter.faction;
                if (actor.ChangeFactionTo(factionToJoinTo)) {
                    Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Join Faction", actor.interruptComponent.identifier);
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(factionToJoinTo, factionToJoinTo.name, LOG_IDENTIFIER.FACTION_1);
                    actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                    return true;
                }
            }
            return base.ExecuteInterruptStartEffect(actor, target);
        }
        #endregion
    }
}