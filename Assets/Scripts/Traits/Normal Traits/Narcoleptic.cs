using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Narcoleptic : Trait {
        public Character owner { get; private set; }

        public Narcoleptic() {
            name = "Narcoleptic";
            description = "Narcoleptics may sometimes suddenly fall asleep.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.NARCOLEPTIC_NAP };
            
            daysDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                owner = sourceCharacter as Character;
            }
        }
        public override bool PerTickOwnerMovement() {
            int napChance = UnityEngine.Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (napChance < 1) {
                if (owner.currentActionNode == null || (owner.currentActionNode.action.goapType != INTERACTION_TYPE.NARCOLEPTIC_NAP)) {
                    DoNarcolepticNap();

                    hasCreatedJob = true;
                }
            }
            return hasCreatedJob;
        }
        public override string TriggerFlaw(Character character) {
            DoNarcolepticNap();
            return base.TriggerFlaw(character);
        }
        private void DoNarcolepticNap() {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.NARCOLEPTIC_NAP, owner, owner);
            owner.jobQueue.AddJobInQueue(job);
        }
        #endregion
    }
}

