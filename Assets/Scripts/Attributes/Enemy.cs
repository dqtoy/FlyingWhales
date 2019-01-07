using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : Trait {
    public Character targetCharacter;

    public override string nameInUI {
        get { return "Enemy: " + targetCharacter.name; }
    }

    public Enemy(Character target) {
        targetCharacter = target;
        name = "Enemy";
        description = "This character is an enemy of " + targetCharacter.name;
        type = TRAIT_TYPE.NEGATIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
