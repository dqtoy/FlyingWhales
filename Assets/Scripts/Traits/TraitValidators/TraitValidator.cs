using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Class used to validate traits for all traitables
    /// </summary>
    public static class TraitValidator {

        public static bool CanAddTrait(ITraitable obj, Trait trait) {
            //Cannot add trait if there is an existing trait that is mutually exclusive of the trait to be added
            if (trait.mutuallyExclusive != null) {
                for (int i = 0; i < trait.mutuallyExclusive.Length; i++) {
                    if (obj.traitContainer.GetNormalTrait(trait.mutuallyExclusive[i]) != null) {
                        return false;
                    }
                }
            }
            //Cannot add trait if it is unique and the character already has that type of trait.
            if (trait.IsUnique()) {
                Trait oldTrait = obj.traitContainer.GetNormalTrait(trait.name);
                if (oldTrait != null) {
                    return false;
                }
            }

            return true;
        }
    }
}

