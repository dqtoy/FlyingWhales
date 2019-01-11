﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charmed : Trait {
    public Faction originalFaction { get; private set; }

    public Charmed(Faction originalFaction) {
        name = "Charmed";
        this.originalFaction = originalFaction;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been bewitched!";
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
