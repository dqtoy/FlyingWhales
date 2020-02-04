using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class SpecialTokenTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
        }
        public override void OnTraitStacked(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            DefaultProcessOnStackTrait(traitable, trait, characterResponsible, gainedFromDoing);
        }
        public override void OnTraitUnstack(ITraitable traitable, Trait trait, Character removedBy = null) {
            DefaultProcessOnUnstackTrait(traitable, trait, removedBy);
        }
    }
}