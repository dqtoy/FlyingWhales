﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Servant : RelationshipTrait {

    public override string nameInUI {
        get { return "Servant: " + targetCharacter.name; }
    }

    public Servant(Character target) : base(target) {
        name = "Servant";
        description = "This character is a servant of " + targetCharacter.name;
        relType = RELATIONSHIP_TRAIT.SERVANT;
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }
}