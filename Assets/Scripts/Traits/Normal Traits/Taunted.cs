using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Taunted : Trait {
        private Character _sourceCharacter;

        public Taunted() {
            name = "Taunted";
            description = "This character is taunted.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            daysDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            _sourceCharacter = sourcePOI as Character;
            if (!_sourceCharacter.isInCombat) {
                _sourceCharacter.marker.AddHostileInRange(responsibleCharacter, false);
            } else {
                Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, _sourceCharacter);
            }
            Messenger.AddListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
            Messenger.AddListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            base.OnRemoveTrait(sourcePOI, removedBy);
            //Character character = sourcePOI as Character;
            Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
            Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
            //if (!character.isDead && character.stateComponent.currentState is CombatState) {
            //    Messenger.Broadcast(Signals.DETERMINE_COMBAT_REACTION, character);
            //}
        }
        public override void OnDeath(Character character) {
            base.OnDeath(character);
            Messenger.RemoveListener<Character, Trait>(Signals.TRAIT_ADDED, OnCharacterGainedTrait);
            Messenger.RemoveListener<Character, Character>(Signals.CHARACTER_REMOVED_FROM_VISION, OnCharacterRemovedFromVision);
            _sourceCharacter.traitContainer.RemoveTrait(_sourceCharacter, this);
        }
        #endregion

        private void OnCharacterGainedTrait(Character character, Trait trait) {
            if (responsibleCharacter.id == character.id) {
                if (character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE) || character.isDead) {
                    _sourceCharacter.traitContainer.RemoveTrait(_sourceCharacter, this); //if the character that taunted this character becomes dead or negatively disabled, remove this trait.
                }
            }
        }
        private void OnCharacterRemovedFromVision(Character character, Character removedCharacter) {
            if (character == _sourceCharacter && removedCharacter == responsibleCharacter) {
                _sourceCharacter.traitContainer.RemoveTrait(_sourceCharacter, this);
            }
        }
    }
}

