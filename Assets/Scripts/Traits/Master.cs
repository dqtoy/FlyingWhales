using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Master : RelationshipTrait {
        public override string nameInUI {
            get { return "Master: " + targetCharacter.name; }
        }

        public Master(Character target) : base(target) {
            name = "Master";
            description = "This character is a servant of " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
            relType = RELATIONSHIP_TRAIT.MASTER;
            associatedInteraction = INTERACTION_TYPE.NONE;
            daysDuration = 0;
            //effects = new List<TraitEffect>();
        }

        public override bool IsUnique() {
            return false;
        }
    }
}

