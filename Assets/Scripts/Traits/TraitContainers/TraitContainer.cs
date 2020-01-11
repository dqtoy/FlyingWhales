﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class TraitContainer : ITraitContainer {

        private List<Trait> _allTraits;

        #region getters/setters
        public List<Trait> allTraits { get { return _allTraits; } }
        public Dictionary<Trait, int> stacks { get; private set; }
        #endregion

        public TraitContainer() {
            _allTraits = new List<Trait>();
            stacks = new Dictionary<Trait, int>();
        }

        #region Adding
        /// <summary>
        /// The main AddTrait function. All other AddTrait functions will eventually call this.
        /// </summary>
        /// <returns>If the trait was added or not.</returns>
        public bool AddTrait(ITraitable addTo, Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            //TODO: Either move or totally remove validation from inside this container
            if (TraitValidator.CanAddTrait(addTo, trait, this) == false) {
                //if (trait.IsUnique()) {
                //    Trait oldTrait = GetNormalTrait<Trait>(trait.name);
                //    if (oldTrait != null) {
                //        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                //        oldTrait.AddCharacterResponsibleForTrait(characterResponsible);
                //        //if (oldTrait.broadcastDuplicates) {
                //        //    Messenger.Broadcast(Signals.TRAIT_ADDED, this, oldTrait);
                //        //}
                //    }
                //}
                return false;
            }

            if (trait.isStacking) {
                if (stacks.ContainsKey(trait)) {
                    stacks[trait]++;
                    addTo.traitProcessor.OnTraitStacked(addTo, trait, characterResponsible, gainedFromDoing);
                } else {
                    stacks.Add(trait, 1);
                    _allTraits.Add(trait);
                    addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing);
                }
            } else {
                _allTraits.Add(trait);
                addTo.traitProcessor.OnTraitAdded(addTo, trait, characterResponsible, gainedFromDoing);
            }
            return true;
        }
        public bool AddTrait(ITraitable addTo, string traitName, out Trait trait, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
            if (TraitManager.Instance.IsInstancedTrait(traitName)) {
                trait = TraitManager.Instance.CreateNewInstancedTraitClass(traitName);
                return AddTrait(addTo, trait, characterResponsible, gainedFromDoing);
            } else {
                trait = TraitManager.Instance.allTraits[traitName];
                return AddTrait(addTo, trait, characterResponsible, gainedFromDoing);
            }
        }
        public bool AddTrait(ITraitable addTo, string traitName, Character characterResponsible = null, ActualGoapNode gainedFromDoing = null) {
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
            Trait trait = GetNormalTrait<Trait>(traitName);
            if (trait != null) {
                return RemoveTrait(removeFrom, trait, removedBy);
            }
            return false;
        }
        public bool RemoveTrait(ITraitable removeFrom, int index, Character removedBy = null) {
            bool removed = true;
            if(index < 0 || index >= _allTraits.Count) {
                removed = false;
            } else {
                Trait trait = _allTraits[index];
                _allTraits.RemoveAt(index);
                removeFrom.traitProcessor.OnTraitRemoved(removeFrom, trait, removedBy);
            }
            return removed;
        }
        public void RemoveTrait(ITraitable removeFrom, List<Trait> traits) {
            for (int i = 0; i < traits.Count; i++) {
                RemoveTrait(removeFrom, traits[i]);
            }
        }
        public List<Trait> RemoveAllTraitsByType(ITraitable removeFrom, TRAIT_TYPE traitType) {
            List<Trait> removedTraits = new List<Trait>();
            //List<Trait> all = new List<Trait>(allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
                if (trait.type == traitType) {
                    removedTraits.Add(trait);
                    if(RemoveTrait(removeFrom, i)) {
                        i--;
                    }
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
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (!currTrait.isPersistent) {
                    if(RemoveTrait(traitable, i)) {
                        i--;
                    }
                }
            }
        }
        public void RemoveAllTraits(ITraitable traitable) {
            //List<Trait> allTraits = new List<Trait>(this.allTraits);
            for (int i = 0; i < allTraits.Count; i++) {
                if (RemoveTrait(traitable, i)) { //remove all traits
                    i--;
                }
            }
        }
        #endregion

        #region Getting
        public T GetNormalTrait<T>(params string[] traitNames) where T : Trait {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait trait = allTraits[i];
                for (int j = 0; j < traitNames.Length; j++) {
                    if (trait.name == traitNames[j]) { // || trait.GetType().ToString() == traitNames[j]
                        return trait as T;
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
        public bool HasTraitOf(TRAIT_EFFECT traitEffect) {
            for (int i = 0; i < allTraits.Count; i++) {
                Trait currTrait = allTraits[i];
                if (currTrait.effect == traitEffect) {
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

        #region Processes
        public void ProcessOnTickStarted() {
            if(allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    allTraits[i].OnTickStarted();
                }
            }
        }
        public void ProcessOnTickEnded() {
            if (allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    allTraits[i].OnTickEnded();
                }
            }
        }
        public void ProcessOnHourStarted() {
            if (allTraits != null) {
                for (int i = 0; i < allTraits.Count; i++) {
                    allTraits[i].OnHourStarted();
                }
            }
        }
        #endregion
    }
}
