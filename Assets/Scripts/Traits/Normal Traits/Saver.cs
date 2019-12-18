using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Saver : RelationshipTrait {

        public override string nameInUI {
            get { return "Saver: " + targetCharacter.name; }
        }

        public Saver(Character target) : base(target) {
            name = "Saver";
            description = "This character is a Saver of " + targetCharacter.name;
            relType = RELATIONSHIP_TYPE.SAVER;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }
    }

}
