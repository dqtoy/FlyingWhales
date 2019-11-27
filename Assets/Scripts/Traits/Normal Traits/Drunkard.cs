using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    public class Drunkard : Trait {

        public Drunkard() {
            name = "Drunkard";
            description = "Drunkards enjoy drinking.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEGATIVE;
            
            
            daysDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override string TriggerFlaw(Character character) {
            //Will drink
            if (!character.jobQueue.HasJob(JOB_TYPE.TRIGGER_FLAW)) {
                bool triggerBrokenhearted = false;
                Heartbroken heartbroken = character.traitContainer.GetNormalTrait("Heartbroken") as Heartbroken;
                if (heartbroken != null) {
                    triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
                }
                if (!triggerBrokenhearted) {
                    if (character.jobQueue.HasJob(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN)) {
                        character.jobQueue.CancelAllJobs(JOB_TYPE.HAPPINESS_RECOVERY, JOB_TYPE.HAPPINESS_RECOVERY_FORLORN);
                    }

                    //TileObject to = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.INN).GetTileObjectsThatAdvertise(INTERACTION_TYPE.DRINK).First();
                    GoapPlanJob drinkJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.DRINK, character, character);
                    character.jobQueue.AddJobInQueue(drinkJob);
                } else {
                    heartbroken.TriggerBrokenhearted();
                }
            }
            return base.TriggerFlaw(character);
        }
        public override void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) {
            base.ExecuteCostModification(action, actor, poiTarget, otherData, ref cost);
            if (action == INTERACTION_TYPE.DRINK) {
                cost =  Utilities.rng.Next(5, 20);
            }
        }
        #endregion
    }

}