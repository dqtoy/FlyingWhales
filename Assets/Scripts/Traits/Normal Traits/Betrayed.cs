using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Betrayed : Trait {

        public Betrayed() {
            name = "Betrayed";
            description = "This character is betrayed.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(12);
            //effects = new List<TraitEffect>();
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_ITEM, INTERACTION_TYPE.CRAFT_FURNITURE };
        }
    }
}

