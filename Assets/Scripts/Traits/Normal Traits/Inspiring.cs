using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Inspiring : Trait {
        public Inspiring() {
            name = "Inspiring";
            description = "Inspring characters make people around them feel happier.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (characterThatWillDoJob.faction == targetCharacter.faction || characterThatWillDoJob.homeSettlement == targetCharacter.homeSettlement) {
                    if (UnityEngine.Random.Range(0, 100) < 8) {
                        targetCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Inspired, characterThatWillDoJob);
                        return true;
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}