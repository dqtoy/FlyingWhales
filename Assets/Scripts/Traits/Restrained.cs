using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Restrained : Trait {
    private Character _responsibleCharacter;

    #region getters/setters
    public override Character responsibleCharacter {
        get { return _responsibleCharacter; }
    }
    #endregion

    public Restrained() {
        name = "Restrained";
        description = "This character is restrained!";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        _responsibleCharacter = character;
    }
    public override string GetToolTipText() {
        return "This character is restrained by " + _responsibleCharacter.name;
    }
    #endregion
}
