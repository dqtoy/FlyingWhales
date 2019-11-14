using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Paramour : RelationshipTrait {

        public override string nameInUI {
            get { return "Paramour: " + targetCharacter.name; }
        }

        public Paramour(Character target) : base(target) {
            name = "Paramour";
            description = "This character is a paramour of " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
            relType = RELATIONSHIP_TRAIT.PARAMOUR;
            
            daysDuration = 0;
            //effects = new List<TraitEffect>();
        }
    }

}
