using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Assaulter : Criminal {
        public override bool isPersistent { get { return true; } }
        public Assaulter() {
            name = "Assaulter";
            description = "This character has been branded as a Assaulter by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            daysDuration = 0;
            crimeSeverity = CRIME_CATEGORY.MISDEMEANOR;
        }
    }
}