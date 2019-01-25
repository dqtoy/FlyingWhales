using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : RelationshipTrait {
    public override string nameInUI {
        get { return "Master: " + targetCharacter.name; }
    }

    public Master(Character target) : base (target){
        name = "Master";
        description = "This character is a master of " + targetCharacter.name;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        relType = RELATIONSHIP_TRAIT.MASTER;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}
