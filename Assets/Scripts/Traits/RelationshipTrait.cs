using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationshipTrait : Trait {
    public Character targetCharacter { get; private set; }

    public RELATIONSHIP_TRAIT relType { get; protected set; }

    public RelationshipTrait(Character target) {
        targetCharacter = target;
        name = "Relationship";
        description = "This character has a relationship with " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.POSITIVE;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
