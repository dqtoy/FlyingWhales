using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Wet : Trait {

        public Wet() {
            name = "Wet";
            description = "This character is soaking wet.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            ticksDuration = 10; //if this trait is only temporary, then it should not advertise GET_WATER
            effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            addedTo.traitContainer.RemoveTrait(addedTo, "Burning");
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
        }
        #endregion


    }
}
