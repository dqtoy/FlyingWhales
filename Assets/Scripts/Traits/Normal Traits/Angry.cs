using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Angry : Trait {
        public Angry() {
            name = "Angry";
            description = "This character will often argue with others and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        //public override void OnAddTrait(ITraitable sourceCharacter) {
        //    base.OnAddTrait(sourceCharacter);
        //    if (sourceCharacter is Character) {
        //        (sourceCharacter as Character).AdjustMoodValue(-5, this);
        //    }
        //}
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject) {
                TileObject tileObject = targetPOI as TileObject;
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DESTROY, tileObject)) {
                        GoapPlanJob destroyJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.ASSAULT, tileObject, characterThatWillDoJob);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(destroyJob);
                        return true;
                    }
                }
            } else if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (UnityEngine.Random.Range(0, 2) == 0 && characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, isLethal: false);
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

