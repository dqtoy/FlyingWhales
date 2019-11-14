using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Vigilant : Trait {

        public Vigilant() {
            name = "Vigilant";
            description = "Vigilant characters cannot be stealthily knocked out or pickpocketed.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            daysDuration = 0;
        }
    }
}

