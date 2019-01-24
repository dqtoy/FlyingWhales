﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charmed : Trait {
    public Faction originalFaction { get; private set; }
    public Area originalHome { get; private set; }

    public Charmed(Faction originalFaction, Area originalHome) {
        name = "Charmed";
        this.originalFaction = originalFaction;
        this.originalHome = originalHome;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been bewitched!";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnRemoveTrait(Character sourceCharacter) {
        base.OnRemoveTrait(sourceCharacter);
        sourceCharacter.ReturnToOriginalHomeAndFaction(originalHome, originalFaction);
    }
    #endregion
}