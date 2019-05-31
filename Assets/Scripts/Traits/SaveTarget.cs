using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveTarget : RelationshipTrait {

    public override string nameInUI {
        get { return "Save Target: " + targetCharacter.name; }
    }

    public SaveTarget(Character target) : base(target) {
        name = "Save Target";
        description = "This character is a Save Target of " + targetCharacter.name;
        relType = RELATIONSHIP_TRAIT.SAVE_TARGET;
        type = TRAIT_TYPE.RELATIONSHIP;
        effect = TRAIT_EFFECT.NEUTRAL;
        associatedInteraction = INTERACTION_TYPE.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
