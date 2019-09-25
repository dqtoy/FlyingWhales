﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Angry : Trait {
    public Angry() {
        name = "Angry";
        description = "This character is angry";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
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
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}