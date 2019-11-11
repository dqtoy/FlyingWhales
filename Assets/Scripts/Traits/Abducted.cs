using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Abducted : Trait {
        public Region originalHome { get; private set; }

        public Abducted(Region originalHome) {
            name = "Abducted";
            SetOriginalHome(originalHome);
            //name = "Charmed from " + originalFaction.name;
            description = "This character has been abducted!";
            thoughtText = "[Character] has been abducted.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.RELEASE_CHARACTER };
            daysDuration = 0;
            //effects = new List<TraitEffect>();
        }

        public void SetOriginalHome(Region origHome) {
            originalHome = origHome;
        }

        #region Overrides
        public override string GetToolTipText() {
            if (responsibleCharacter == null) {
                return description;
            }
            return "This character has been abducted by " + responsibleCharacter.name;
        }
        #endregion
    }

    public class SaveDataAbducted : SaveDataTrait {
        public int originalHomeID;

        public override void Save(Trait trait) {
            base.Save(trait);
            Abducted derivedTrait = trait as Abducted;
            originalHomeID = derivedTrait.originalHome.id;
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            Abducted derivedTrait = trait as Abducted;
            Region origHome = GridMap.Instance.GetRegionByID(originalHomeID);
            derivedTrait.SetOriginalHome(origHome);
            return trait;
        }
    }
}