using System.Collections.Generic;

namespace Traits {
    public class Aberration : Criminal {
        public override bool isPersistent { get { return true; } }
        public Aberration() {
            name = "Aberration";
            description = "This character has been branded as an Aberration by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //crimeSeverity = CRIME_TYPE.SERIOUS;
        }
    }
}
