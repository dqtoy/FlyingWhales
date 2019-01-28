using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abducted : Trait {
    public Area originalHome { get; private set; }

    public Abducted(Area originalHome) {
        name = "Abducted";
        this.originalHome = originalHome;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been abducted!";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
	
}
