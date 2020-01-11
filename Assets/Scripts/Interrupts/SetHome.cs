﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class SetHome : Interrupt {
        public SetHome() : base(INTERRUPT.Set_Home) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            if(actor.homeSettlement != null) {
                IDwelling chosenHomeStructure = null;
                if(target != actor) {
                    chosenHomeStructure = (target as Character).homeStructure;
                }
                actor.homeSettlement.AssignCharacterToDwellingInArea(actor, chosenHomeStructure);
                if(actor.homeStructure != null) {
                    Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Set Home", "set_new_home_structure");
                    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, actor.homeStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.STRING_1);
                    actor.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                }
            }
            return true;
        }
        #endregion
    }
}