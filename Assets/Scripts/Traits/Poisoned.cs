using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoned : Trait {

    public List<Character> responsibleCharacters { get; private set; }

    public Poisoned() {
        name = "Poisoned";
        description = "This character is poisoned.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        responsibleCharacters = new List<Character>();
    }

    #region Overrides
    public override void SetCharacterResponsibleForTrait(Character character) {
        if (!responsibleCharacters.Contains(character)) {
            responsibleCharacters.Add(character);
        }
    }
    public override bool IsResponsibleForTrait(Character character) {
        return responsibleCharacters.Contains(character);
    }
    #endregion   
}
