using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Glutton : Trait {

        private int additionalFullnessDecreaseRate;

        public Glutton() {
            name = "Glutton";
            description = "Gluttons consume more food than normal.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                additionalFullnessDecreaseRate = Mathf.CeilToInt(CharacterManager.FULLNESS_DECREASE_RATE * 0.5f);
                Character character = addedTo as Character;
                character.needsComponent.SetFullnessForcedTick(0);
                character.needsComponent.AdjustFullnessDecreaseRate(additionalFullnessDecreaseRate);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.needsComponent.SetFullnessForcedTick();
                character.needsComponent.AdjustFullnessDecreaseRate(-additionalFullnessDecreaseRate);
            }
        }
        public override string TriggerFlaw(Character character) {
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                //Will perform Fullness Recovery.
                character.TriggerFlawFullnessRecovery();
            }
            return base.TriggerFlaw(character);
        }
        #endregion
    }
}
