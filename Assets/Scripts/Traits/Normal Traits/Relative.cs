using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Relative : RelationshipTrait {
        public override string nameInUI {
            get { return "Relative: " + targetCharacter.name; }
        }

        public Relative(Character target) : base(target) {
            name = "Relative";
            description = "This character is a relative of " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.NEUTRAL;
            relType = RELATIONSHIP_TRAIT.RELATIVE;
            
            daysDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region overrides
        public override bool IsUnique() {
            return false;
        }
        #endregion
    }
}

