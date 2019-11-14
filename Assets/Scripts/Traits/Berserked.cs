using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Berserked : Trait {

        public override bool isNotSavable {
            get { return true; }
        }

        public Berserked() {
            name = "Berserked";
            description = "This character will attack anyone at random and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                if (character.marker != null) {
                    character.marker.BerserkedMarker();
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                if (character.marker != null) {
                    character.marker.UnberserkedMarker();
                }
            }
        }
        #endregion
    }
}

