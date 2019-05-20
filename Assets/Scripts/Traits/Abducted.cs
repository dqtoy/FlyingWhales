using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Abducted : Trait {
    public Area originalHome { get; private set; }

    private Character _responsibleCharacter;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Abducted(Area originalHome) {
        name = "Abducted";
        this.originalHome = originalHome;
        //name = "Charmed from " + originalFaction.name;
        description = "This character has been abducted!";
        thoughtText = "[Character] has been abducted.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.RELEASE_ABDUCTED_ACTION };
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override bool IsResponsibleForTrait(Character character) {
        return _responsibleCharacter == character;
    }
    public override string GetToolTipText() {
        if(_responsibleCharacter == null) {
            return description;
        }
        return "This character has been abducted by " + _responsibleCharacter.name;
    }
    #endregion
}
