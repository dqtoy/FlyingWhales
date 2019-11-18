using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class SpecialTokenTraitProcessor : TraitProcessor {

        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, GoapAction gainedFromDoing = null) {
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
        }

        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
        }
    }
}