using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Catatonic : Trait {

    public Catatonic() {
        name = "Catatonic";
        description = "This character is catatonic.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        daysDuration = GameManager.ticksPerDay;
    }
}
