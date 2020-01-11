using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Servant : RelationshipTrait {

        //public override string nameInUI {
        //    get { return "Servant: " + targetCharacter.name; }
        //}

        public Servant(Character target) : base(target) {
            name = "Servant";
            description = "This character is a master of " + targetCharacter.name;
            relType = RELATIONSHIP_TYPE.SERVANT;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        public override bool IsUnique() {
            return false;
        }
        public override string GetNameInUI(ITraitable traitable) {
            return "Servant: " + traitable.name;
        }
    }
}

