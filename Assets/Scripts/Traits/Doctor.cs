using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Doctor : Trait {
        public Doctor() {
            name = "Doctor";
            description = "Doctors can craft Healing Potions and help injured and sick characters.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
            associatedInteraction = INTERACTION_TYPE.NONE;
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.CRAFT_ITEM };
        }
    }
}
