using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Lover : RelationshipTrait {

        public override string nameInUI {
            get { return "Lover: " + targetCharacter.name; }
        }

        public Lover(Character target) : base(target) {
            name = "Lover";
            description = "This character is a lover of " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
            relType = RELATIONSHIP_TRAIT.LOVER;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

    }
}