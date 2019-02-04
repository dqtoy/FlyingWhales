using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charmed : Trait {
    public Faction originalFaction { get; private set; }
    public Area originalHome { get; private set; }

    private Character _responsibleCharacter;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Charmed(Faction originalFaction, Area originalHome) {
        name = "Charmed";
        this.originalFaction = originalFaction;
        this.originalHome = originalHome;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been bewitched!";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnRemoveTrait(Character sourceCharacter) {
        base.OnRemoveTrait(sourceCharacter);
        sourceCharacter.ReturnToOriginalHomeAndFaction(originalHome, originalFaction);
    }
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override string GetToolTipText() {
        return "This character has been charmed by " + _responsibleCharacter.name;
    }
    #endregion
}
