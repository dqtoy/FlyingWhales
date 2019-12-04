using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class DefaultTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
        }
    }

}
