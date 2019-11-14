using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Disillusioned : Trait {

        public Disillusioned() {
            name = "Disillusioned";
            description = "Disillusioned characters have given up on the world.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            string logKey = base.TriggerFlaw(character);
            if (character.faction != FactionManager.Instance.neutralFaction) {
                character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
                return logKey;
            } else {
                return "fail";
            }
        }
        #endregion
    }
}