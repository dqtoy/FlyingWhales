using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Charmed : Trait {
        public Faction originalFaction { get; private set; }
        public Region originalHome { get; private set; }

        public Charmed(Faction originalFaction, Region originalHome) {
            name = "Charmed";
            this.originalFaction = originalFaction;
            this.originalHome = originalHome;
            //name = "Charmed from " + originalFaction.name;
            description = "This character has been bewitched!";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            if (sourcePOI is Character) {
                (sourcePOI as Character).ReturnToOriginalHomeAndFaction(originalHome, originalFaction);
            }
        }
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
            }
            return "This character has been charmed by " + responsibleCharacter.name;
        }
        #endregion
    }
}

