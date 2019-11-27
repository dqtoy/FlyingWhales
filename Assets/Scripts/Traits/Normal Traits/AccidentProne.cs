using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class AccidentProne : Trait {

        public static INTERACTION_TYPE[] excludedActionsFromAccidentProneTrait = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.STUMBLE, INTERACTION_TYPE.PUKE, INTERACTION_TYPE.SEPTIC_SHOCK, INTERACTION_TYPE.ACCIDENT
        };

        public Character owner { get; private set; }
        public CharacterState storedState { get; private set; }

        public AccidentProne() {
            name = "Accident Prone";
            description = "Accident Prone characters often gets injured.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.ACCIDENT, INTERACTION_TYPE.STUMBLE };
            
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
            int stumbleChance = UnityEngine.Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (stumbleChance < 2) {
                if (owner.currentActionNode == null || (owner.currentActionNode.action.goapType != INTERACTION_TYPE.STUMBLE && owner.currentActionNode.action.goapType != INTERACTION_TYPE.ACCIDENT)) {
                    DoStumble();
                    hasCreatedJob = true;
                }
            }
            return hasCreatedJob;
        }
        public override bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) {
            int accidentChance = UnityEngine.Random.Range(0, 100);
            bool hasCreatedJob = false;
            if (accidentChance < 10) {
                if (node != null && !excludedActionsFromAccidentProneTrait.Contains(node.action.goapType)) {
                    DoAccident(node.action);
                    hasCreatedJob = true;
                    willStillContinueAction = false;
                }
            }
            return hasCreatedJob;
        }
        public override string TriggerFlaw(Character character) {
            if (character.marker.isMoving) {
                //If moving, the character will stumble and get injured.
                DoStumble();
            } else if (character.currentActionNode != null && !excludedActionsFromAccidentProneTrait.Contains(character.currentActionNode.action.goapType)) {
                //If doing something, the character will fail and get injured.
                DoAccident(character.currentActionNode.action);
            }
            return base.TriggerFlaw(character);
        }
        #endregion

        private void DoStumble() {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.STUMBLE, owner, owner);
            job.SetCannotBePushedBack(true);
            owner.jobQueue.AddJobInQueue(job);
        }

        private void DoAccident(GoapAction action) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.INTERRUPTION, INTERACTION_TYPE.ACCIDENT, owner, owner);
            job.AddOtherData(INTERACTION_TYPE.ACCIDENT, new object[] { action });
            job.SetCannotBePushedBack(true);
            owner.jobQueue.AddJobInQueue(job);
        }
    }
}

