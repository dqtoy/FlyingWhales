using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Functions to be used to determine what happens when a trait is added/removed to a character
    /// </summary>
    public class CharacterTraitProcessor : TraitProcessor {
        public override void OnTraitAdded(ITraitable traitable, Trait trait, Character characterResponsible = null, GoapAction gainedFromDoing = null) {
            Character character = traitable as Character;
            character.ApplyTraitEffects(trait);
            character.ApplyPOITraitInteractions(trait);
            character.currentAlterEgo.AddTrait(trait);

            if (GameManager.Instance.gameHasStarted) {
                if (trait.name == "Starving") {
                    character.PlanFullnessRecoveryActions(true);
                } else if (trait.name == "Forlorn" || trait.name == "Lonely") {
                    character.PlanHappinessRecoveryActions(true);
                } else if (trait.name == "Exhausted") {
                    character.PlanTirednessRecoveryActions(true);
                }
            }
            if (trait.type == TRAIT_TYPE.CRIMINAL
                || (trait.effect == TRAIT_EFFECT.NEGATIVE && trait.type == TRAIT_TYPE.DISABLER)) {
                //when a character gains a criminal trait, drop all location jobs that this character is assigned to
                character.homeArea.jobQueue.UnassignAllJobsTakenBy(character);
            }
            DefaultProcessOnAddTrait(traitable, trait, characterResponsible, gainedFromDoing);
            Messenger.Broadcast(Signals.TRAIT_ADDED, character, trait);
        }

        public override void OnTraitRemoved(ITraitable traitable, Trait trait, Character removedBy) {
            Character character = traitable as Character;
            character.UnapplyTraitEffects(trait);
            character.UnapplyPOITraitInteractions(trait);
            character.currentAlterEgo.traits.Remove(trait);

            DefaultProcessOnRemoveTrait(traitable, trait, removedBy);
            Messenger.Broadcast(Signals.TRAIT_REMOVED, character, trait);
        }
    }
}

