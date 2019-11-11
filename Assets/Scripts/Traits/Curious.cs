using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Curious : Trait {
        //public List<TileObject> alreadyInspectedTileObjects { get; private set; }

        public Curious() {
            name = "Curious";
            description = "This character is curious.";
            type = TRAIT_TYPE.PERSONALITY;
            effect = TRAIT_EFFECT.NEUTRAL;
            trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
            associatedInteraction = INTERACTION_TYPE.NONE;
            crimeSeverity = CRIME_CATEGORY.NONE;
            daysDuration = 0;
            //alreadyInspectedTileObjects = new List<TileObject>();
            //effects = new List<TraitEffect>();
        }

        //public void AddAlreadyInspectedObject(TileObject to) {
        //    if (!alreadyInspectedTileObjects.Contains(to)) {
        //        alreadyInspectedTileObjects.Add(to);
        //    }
        //}
        //#region Overrides
        //public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        //    if (targetPOI is TileObject) {
        //        TileObject objectToBeInspected = targetPOI as TileObject;
        //        if(objectToBeInspected.isSummonedByPlayer && !alreadyInspectedTileObjects.Contains(objectToBeInspected)) {
        //            if(!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, objectToBeInspected)){
        //                GoapPlanJob inspectJob = new GoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, objectToBeInspected);
        //                characterThatWillDoJob.jobQueue.AddJobInQueue(inspectJob);
        //            }
        //        }
        //    }
        //    return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        //}
        //#endregion
    }

}
