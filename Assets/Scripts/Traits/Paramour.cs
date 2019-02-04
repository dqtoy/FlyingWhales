using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Paramour : RelationshipTrait {

    public override string nameInUI {
        get { return "Paramour: " + targetCharacter.name; }
    }

    public Paramour(Character target) : base(target) {
        name = "Paramour";
        description = "This character is a paramour of " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.POSITIVE;
        relType = RELATIONSHIP_TRAIT.PARAMOUR;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
