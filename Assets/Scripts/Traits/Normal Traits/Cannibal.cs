using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Cannibal : Trait {

        public Cannibal() {
            name = "Cannibal";
            description = "This character eats his own kind.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            
            daysDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            base.OnAddTrait(sourcePOI);
            if (sourcePOI is Character) {
                Character owner = sourcePOI as Character;
                GoapPlanJob job = owner.jobQueue.GetJob(JOB_TYPE.HUNGER_RECOVERY, JOB_TYPE.HUNGER_RECOVERY_STARVING) as GoapPlanJob;
                if (job != null) {
                    owner.jobQueue.CancelJob(job, shouldDoAfterEffect: false);
                }
            }
        }
        //public override void OnRemoveTrait(IPointOfInterest sourcePOI) {
        //    base.OnRemoveTrait(sourcePOI);
        //}
        protected override void OnChangeLevel() {
            base.OnChangeLevel();
            //if (level == 1) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(3);
            //} else if (level == 2) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(6);
            //} else if (level == 3) {
            //    daysDuration = GameManager.Instance.GetTicksBasedOnHour(9);
            //}
        }
        public override string TriggerFlaw(Character character) {
            string successLogKey = base.TriggerFlaw(character);
            IPointOfInterest poi = GetPOIToTransformToFood(character);
            if (poi != null) {
                GoapPlanJob job = new GoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.TRANSFORM_FOOD, poi);
                character.jobQueue.AddJobInQueue(job);
                return successLogKey;
            } else {
                return "fail";
            }
        }
        #endregion

        private IPointOfInterest GetPOIToTransformToFood(Character characterThatWillDoJob) {
            IPointOfInterest chosenPOI = null;
            for (int i = 0; i < characterThatWillDoJob.specificLocation.charactersAtLocation.Count; i++) {
                Character character = characterThatWillDoJob.specificLocation.charactersAtLocation[i];
                if (characterThatWillDoJob != character && character.isDead) {
                    if (character.grave != null) {
                        chosenPOI = character.grave;
                    } else {
                        chosenPOI = character;
                    }
                    break;
                }
            }

            if (chosenPOI == null) {
                for (int i = 0; i < characterThatWillDoJob.specificLocation.charactersAtLocation.Count; i++) {
                    Character character = characterThatWillDoJob.specificLocation.charactersAtLocation[i];
                    if (characterThatWillDoJob != character && characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(character.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
                        chosenPOI = character;
                        break;
                    }
                }
            }

            if (chosenPOI == null) {
                for (int i = 0; i < characterThatWillDoJob.specificLocation.charactersAtLocation.Count; i++) {
                    Character character = characterThatWillDoJob.specificLocation.charactersAtLocation[i];
                    if (characterThatWillDoJob != character && characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(character.currentAlterEgo) == RELATIONSHIP_EFFECT.NONE) {
                        chosenPOI = character;
                        break;
                    }
                }
            }
            return chosenPOI;
        }
    }
}

