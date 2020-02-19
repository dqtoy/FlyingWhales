using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Wet : Trait {

        public Wet() {
            name = "Wet";
            description = "This is soaking wet.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(2); //if this trait is only temporary, then it should not advertise GET_WATER
            isStacking = true;
            moodEffect = -6;
            stackLimit = 10;
            stackModifier = 0f;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            addedTo.traitContainer.RemoveTrait(addedTo, "Burning");
            addedTo.traitContainer.RemoveTraitAndStacks(addedTo, "Overheating");
            if (addedTo is Character) {
                (addedTo as Character).needsComponent.AdjustComfortDecreaseRate(2f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                (removedFrom as Character).needsComponent.AdjustComfortDecreaseRate(-2f);
            }
        }
        #endregion


    }
}
