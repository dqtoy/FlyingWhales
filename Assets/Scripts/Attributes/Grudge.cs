using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grudge : Trait {
    public Character targetCharacter;

    public override string nameInUI {
        get { return "Grudge: " + targetCharacter.name;}
    }

    public Grudge(Character target) {
        targetCharacter = target;
        name = "Grudge";
        description = "This character holds a grudge against " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
