using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class RelationshipTrait : Trait {
        public Character targetCharacter { get; private set; }

        public RELATIONSHIP_TRAIT relType { get; protected set; }
        public int severity { get; protected set; }

        private const int MIN_SEVERITY = 1;
        private const int MAX_SEVERITY = 3;

        public RelationshipTrait(Character target) {
            targetCharacter = target;
            name = "Relationship";
            description = "This character has a relationship with " + targetCharacter.name;
            type = TRAIT_TYPE.RELATIONSHIP;
            effect = TRAIT_EFFECT.POSITIVE;
            
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
            severity = MIN_SEVERITY;
        }

        #region Severity
        public void AdjustSeverity(int amount) {
            severity += amount;
            severity = Mathf.Clamp(severity, MIN_SEVERITY, MAX_SEVERITY);
        }
        public void SetSeverity(int amount) {
            severity = amount;
            severity = Mathf.Clamp(severity, MIN_SEVERITY, MAX_SEVERITY);
        }
        #endregion
    }
}

