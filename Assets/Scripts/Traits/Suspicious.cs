﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Suspicious : Trait {

    public Suspicious() {
        name = "Suspicious";
        description = "This character is suspicious.";
        type = TRAIT_TYPE.BUFF;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is TileObject) {
            TileObject objectToBeInspected = targetPOI as TileObject;
            if (objectToBeInspected.isSummonedByPlayer) {
                if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.DESTROY, objectToBeInspected)) {
                    GoapPlanJob destroyJob = new GoapPlanJob(JOB_TYPE.DESTROY, INTERACTION_TYPE.TILE_OBJECT_DESTROY, objectToBeInspected);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(destroyJob);
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}