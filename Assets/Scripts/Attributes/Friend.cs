using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Friend : Trait {
    public Character targetCharacter;

    public override string nameInUI {
        get { return "Friend: " + targetCharacter.name;}
    }

    public Friend(Character target) {
        targetCharacter = target;
        name = "Friend";
        description = "This character is a friend of " + targetCharacter.name;
        type = TRAIT_TYPE.POSITIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
