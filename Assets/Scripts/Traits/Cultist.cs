using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Cultist : Trait {

        public UnsummonedMinionData minionData { get; private set; }

        public Cultist() {
            name = "Cultist";
            description = "Cultists are secret followers of the Ruinarch.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            associatedInteraction = INTERACTION_TYPE.NONE;
            trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;


        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            string deadlySin = CharacterManager.sevenDeadlySinsClassNames[Random.Range(0, CharacterManager.sevenDeadlySinsClassNames.Length)];
            minionData = new UnsummonedMinionData() {
                minionName = addedTo.name,
                className = deadlySin,
                combatAbility = PlayerManager.Instance.allCombatAbilities[UnityEngine.Random.Range(0, PlayerManager.Instance.allCombatAbilities.Length)],
                interventionAbilitiesToResearch = CharacterManager.Instance.Get3RandomResearchInterventionAbilities(CharacterManager.Instance.GetDeadlySin(deadlySin)),
            };
        }
        #endregion
    }
}
