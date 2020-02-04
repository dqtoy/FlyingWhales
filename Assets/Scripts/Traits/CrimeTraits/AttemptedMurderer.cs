using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class AttemptedMurderer : Criminal {
        public override bool isPersistent { get { return true; } }
        public AttemptedMurderer() {
            name = "Attempted Murderer";
            description = "This character has been branded as an Attempted Murderer by his/her own faction.";
            type = TRAIT_TYPE.CRIMINAL;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //crimeSeverity = CRIME_TYPE.MISDEMEANOR;
            //effects = new List<TraitEffect>();
        }
    }
}

