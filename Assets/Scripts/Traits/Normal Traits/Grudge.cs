using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Grudge : Trait {
        public Character targetCharacter;

        //public override string nameInUI {
        //    get { return "Grudge: " + targetCharacter.name; }
        //}

        public Grudge() {
            name = "Grudge";
            description = "This is a placeholder trait";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        public Grudge(Character target) {
            targetCharacter = target;
            name = "Grudge";
            description = $"This character holds a grudge against {targetCharacter.name}";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        public override string GetNameInUI(ITraitable traitable) {
            return $"Grudge: {traitable.name}";
        }
    }
}

