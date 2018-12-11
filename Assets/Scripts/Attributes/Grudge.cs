using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grudge : Trait {
    public Character targetCharacter;

    public Grudge(Character target) {
        targetCharacter = target;
        name = "Grudge: " + targetCharacter.name;
        description = "This character holds a grudge against " + targetCharacter.name;
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
