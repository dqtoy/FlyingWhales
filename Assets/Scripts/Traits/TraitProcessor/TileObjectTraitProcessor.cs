using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class TileObjectTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            TileObject obj = traitable as TileObject;
            obj.OnTileObjectGainedTrait(trait);
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
           
        }
        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            TileObject obj = traitable as TileObject;
            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
            obj.OnTileObjectLostTrait(trait);
        }
        public override void OnTraitStacked(ITraitable traitable, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            DefaultProcessOnStackTrait(traitable, trait, characterResponsible, gainedFromDoing);
        }
        public override void OnTraitUnstack(ITraitable traitable, Trait trait, Character removedBy = null) {
            DefaultProcessOnUnstackTrait(traitable, trait, removedBy);

        }
    }

}
