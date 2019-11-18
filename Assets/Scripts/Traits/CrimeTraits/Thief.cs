using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Thief : Criminal {

        public override bool isPersistent { get { return true; } }

        public Thief() {
            name = "Thief";
            description = "This character has been branded as a Thief by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            daysDuration = 0;
            crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
        }
    }
}

