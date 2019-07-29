using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisoned : Trait {

    public List<Character> awareCharacters { get; private set; } //characters that know about this trait

    private List<Character> _responsibleCharacters;

    public IPointOfInterest poi { get; private set; } //poi that has the poison
    public Poisoned() {
        name = "Poisoned";
        description = "This object is poisoned.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        _responsibleCharacters = new List<Character>();
        awareCharacters = new List<Character>();
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        base.OnAddTrait(sourceCharacter);
        poi = sourceCharacter;
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter, Character removedBy) {
        base.OnRemoveTrait(sourceCharacter, removedBy);
        awareCharacters.Clear();
        responsibleCharacters.Clear(); //Cleared list, for garbage collection
        //Messenger.Broadcast(Signals.OLD_NEWS_TRIGGER, sourceCharacter, gainedFromDoing);
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
