using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Disabled : Trait {

        public Disabled() {
            name = "Disabled";
            description = "This object is disabled.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            //effects = new List<TraitEffect>();
        }

        public override void OnAddTrait(ITraitable targetPOI) {
            base.OnAddTrait(targetPOI);
            if (targetPOI is IPointOfInterest) {
                (targetPOI as IPointOfInterest).SetIsDisabledByPlayer(true);
            }

        }

        public override void OnRemoveTrait(ITraitable targetPOI, Character removedBy) {
            base.OnRemoveTrait(targetPOI, removedBy);
            if (targetPOI is IPointOfInterest) {
                (targetPOI as IPointOfInterest).SetIsDisabledByPlayer(false);
            }
        }
    }
}

