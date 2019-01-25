using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mentor : RelationshipTrait {
    public override string nameInUI {
        get { return "Mentor: " + targetCharacter.name; }
    }

    public Mentor(Character target) : base (target) {
        name = "Mentor";
        description = "This character is a mentor of " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.POSITIVE;
        relType = RELATIONSHIP_TRAIT.MENTOR;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
