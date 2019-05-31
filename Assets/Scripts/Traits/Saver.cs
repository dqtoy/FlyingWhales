using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saver : RelationshipTrait {

    public override string nameInUI {
        get { return "Saver: " + targetCharacter.name; }
    }

    public Saver(Character target) : base(target) {
        name = "Saver";
        description = "This character is a Saver of " + targetCharacter.name;
        relType = RELATIONSHIP_TRAIT.SAVER;
        type = TRAIT_TYPE.RELATIONSHIP;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
