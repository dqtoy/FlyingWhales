using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class TraitContainer : ITraitContainer {

        private List<Trait> _allTraits;

        #region getters/setters
        public List<Trait> allTraits { get { return _allTraits; } }
        public List<RelationshipTrait> relationshipTraits { get { return new List<RelationshipTrait>(); } }
        #endregion

        public TraitContainer() {
            _allTraits = new List<Trait>();
        }

        #region Adding
        /// <summary>
        /// The main AddTrait function. All other AddTrait functions will eventually call this.
        /// </summary>
        /// <returns>If the trait was added or not.</returns>
        public bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, GoapAction gainedFromDoing = null) {
            //TODO: Either move or totally remove validation from inside this container
            if (TraitValidator.CanAddTrait(addTo, trait) == false) {
                if (trait.IsUnique()) {
                    Trait oldTrait = GetNormalTrait(trait.name);
                    if (oldTrait != null) {
                        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                        //if (oldTrait.broadcastDuplicates) {
                        //    Messenger.Broadcast(Signals.TRAIT_ADDED, this, oldTrait);
                        //}
                    }
                }
                return false;
            }

            _allTraits.Add(trait);
            addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing);
            return true;
        }
        public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, GoapAction gainedFromDoing = null) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                trait = TraitManager.Instance.CreateNewInstancedTraitClass(traitName);
                return AddTrait(addTo, trait, characterResponsible, gainedFromDoing);
            } else {
                trait = TraitManager.Instance.allTraits[traitName];
                return AddTrait(addTo, trait, characterResponsible, gainedFromDoing);
            }
        }
        public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, GoapAction gainedFromDoing = null) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                return AddTrait(addTo, TraitManager.Instance.CreateNewInstancedTraitClass(traitName), characterResponsible, gainedFromDoing);
            } else {
                return AddTrait(addTo, TraitManager.Instance.allTraits[traitName], characterResponsible, gainedFromDoing);
            }
        }
        #endregion

        #region Removing
        /// <summary>
        /// The main RemoveTrait function. All other RemoveTrait functions eventually call this.
        /// </summary>
        /// <returns>If the trait was removed or not.</returns>
        public bool RemoveTrait(ITraitable removeFrom, Trait trait, Character removedBy = null) {
            bool removed = _allTraits.Remove(trait);
            if (removed) {
                removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
            }
            return removed;
        }
        public bool RemoveTrait(ITraitable removeFrom, string traitName, Character removedBy = null) {
            Trait trait = GetNormalTrait(traitName);
            if (trait != null) {
                return RemoveTrait(removeFrom, trait, removedBy);
            }
            return false;
        }
        public void RemoveTrait(ITraitable removeFrom, List<Trait> traits) {
            for (int i = 0; i < traits.Count; i++) {
                RemoveTrait(removeFrom, traits[i]);
            }
        }
        public List<Trait> RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType) {
            List<Trait> removedTraits = new List<Trait>();
            List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < all.Count; i++) {
                Trait trait = all[i];
                if (trait.type == traitType) {
                    removedTraits.Add(trait);
                    RemoveTrait(removeFrom, trait);
                }
            }
            return removedTraits;
        }
        public bool RemoveTraitOnSchedule(ITraitable removeFrom, Trait trait) {
            return RemoveTrait(removeFrom, trait);
        }
        /// <summary>
        /// Remove all traits that are not persistent.
        /// </summary>
        public void RemoveAllNonPersistentTraits(ITraitable traitable) {
            List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (!currTrait.isPersistent) {
                    RemoveTrait(traitable, currTrait);
                }
            }
        }
        public void RemoveAllTraits(ITraitable traitable) {
            List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                RemoveTrait(traitable, allTraits[i]); //remove all traits
            }
        }
        #endregion

        #region Getting
        public Trait GetNormalTrait(params string[] traitNames) {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
                for (int j = 0; j < traitNames.Length; j++) {
                    if (trait.name == traitNames[j] || trait.GetType().ToString() == traitNames[j]) {
                        return trait;
                    }
                }
            }
            return null;
        }
        public bool HasTraitOf(TRAIT_TYPE traitType) {
            for (int i = 0; i < allTraits.Count; i++) {
                if (allTraits[i].type == traitType) {
                    return true;
                }
            }
            return false;
        }
        public bool HasTraitOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    return true;
                }
            }
            return false;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        public List<Trait> GetAllTraitsOf(TRAIT_TYPE type, TRAIT_EFFECT effect) {
            List<Trait> traits = new List<Trait>();
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == effect && currTrait.type == type) {
                    traits.Add(currTrait);
                }
            }
            return traits;
        }
        #endregion
    }
}
