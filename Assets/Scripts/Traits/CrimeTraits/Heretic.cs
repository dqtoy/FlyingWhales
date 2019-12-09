using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Heretic : Criminal {
        public override bool isPersistent { get { return true; } }
        public Heretic() {
            name = "Heretic";
            description = "This character has been branded as a Heretic by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            crimeSeverity = CRIME_CATEGORY.SERIOUS;
        }
    }
}