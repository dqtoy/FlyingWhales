using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abducted : Trait {
    public Faction originalFaction { get; private set; }

    public Abducted(Faction originalFaction) {
        name = "Abducted";
        this.originalFaction = originalFaction;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been abducted!";
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
	
}
