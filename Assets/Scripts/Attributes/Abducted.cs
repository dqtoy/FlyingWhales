using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abducted : Trait {
    public BaseLandmark originalHomeLandmark { get; private set; }

    public Abducted(BaseLandmark originalHomeLandmark) {
        name = "Abducted";
        this.originalHomeLandmark = originalHomeLandmark;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been abducted!";
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
	
}
