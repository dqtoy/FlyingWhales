using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Jolted : Trait {
        public Jolted() {
            name = "Jolted";
            description = "This character is pumped.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            daysDuration = GameManager.Instance.GetTicksBasedOnMinutes(30);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.AdjustSpeedModifier(2f);
            }
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                character.AdjustSpeedModifier(-2f);
            }
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        #endregion
    }
}