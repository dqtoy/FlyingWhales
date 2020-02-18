using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class SaveTarget : RelationshipTrait {

        //public override string nameInUI {
        //    get { return "Save Target: " + targetCharacter.name; }
        //}

        public SaveTarget(Character target) : base(target) {
            name = "Save Target";
            description = $"This character is a Save Target of {targetCharacter.name}";
            relType = RELATIONSHIP_TYPE.SAVE_TARGET;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }
        public override string GetNameInUI(ITraitable traitable) {
            return $"Save Target: {traitable.name}";
        }
    }
}

