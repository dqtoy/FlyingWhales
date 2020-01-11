﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interrupts {
    public class CreateFaction : Interrupt {
        public CreateFaction() : base(INTERRUPT.Create_Faction) {
            duration = 0;
            isSimulateneous = true;
        }

        #region Overrides
        public override bool ExecuteInterruptStartEffect(Character actor, IPointOfInterest target) {
            Faction newFaction = FactionManager.Instance.CreateNewFaction();
            actor.ChangeFactionTo(newFaction);
            newFaction.SetLeader(actor);

            Log createFactionLog = new Log(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction");
            createFactionLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            createFactionLog.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            createFactionLog.AddToFillers(actor.currentRegion, actor.currentRegion.name, LOG_IDENTIFIER.LANDMARK_1);
            actor.RegisterLogAndShowNotifToThisCharacterOnly(createFactionLog, onlyClickedCharacter: false);
            return true;
        }
        #endregion
    }
}