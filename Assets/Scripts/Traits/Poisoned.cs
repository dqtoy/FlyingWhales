using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoned : Trait {

    public List<Character> responsibleCharacters { get; private set; }

    public List<Character> awareCharacters { get; private set; } //characters that know about this trait

    public Poisoned() {
        name = "Poisoned";
        description = "This character is poisoned.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        responsibleCharacters = new List<Character>();
        awareCharacters = new List<Character>();
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
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        base.OnRemoveTrait(sourceCharacter);
        awareCharacters.Clear();
        responsibleCharacters.Clear(); //Cleared list, for garbage collection
    }
    #endregion

    #region Aware Characters
    public void AddAwareCharacter(Character character) {
        if (awareCharacters.Contains(character)) {
            awareCharacters.Add(character);
        }
    }
    public void RemoveAwareCharacter(Character character) {
        awareCharacters.Remove(character);
    }
    #endregion
}
