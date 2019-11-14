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
            
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = GameManager.Instance.GetTicksBasedOnHour(4);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                (sourceCharacter as Character).AdjustMoodValue(-5, this);
            }
        }
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject) {
                TileObject tileObject = targetPOI as TileObject;
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DESTROY, tileObject)) {
                        GoapPlanJob destroyJob = new GoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.TILE_OBJECT_DESTROY, tileObject);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(destroyJob);
                        return true;
                    }
                }
            } else if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (UnityEngine.Random.Range(0, 2) == 0 && characterThatWillDoJob.relationshipContainer.GetRelationshipEffectWith(targetCharacter.currentAlterEgo) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, isLethal: false);
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

