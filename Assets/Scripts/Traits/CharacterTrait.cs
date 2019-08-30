using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This trait is present in all characters
//A dummy trait in order for some jobs to be created
public class CharacterTrait : Trait {
    public List<TileObject> alreadyInspectedTileObjects { get; private set; }

    public CharacterTrait() {
        name = "Character Trait";
        type = TRAIT_TYPE.PERSONALITY;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        isHidden = true;
        alreadyInspectedTileObjects = new List<TileObject>();
    }
    public void AddAlreadyInspectedObject(TileObject to) {
        if (!alreadyInspectedTileObjects.Contains(to)) {
            alreadyInspectedTileObjects.Add(to);
        }
    }

    #region Overrides
    public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
        if (targetPOI is Table) {
            Table targetTable = targetPOI as Table;
            if(targetTable.food < 20 && targetTable.structureLocation is Dwelling) {
                Dwelling dwelling = targetTable.structureLocation as Dwelling;
                if (dwelling.IsResident(characterThatWillDoJob)) {
                    if (!targetTable.HasJobTargettingThis(JOB_TYPE.OBTAIN_FOOD)) {
                        int neededFood = 60 - targetTable.food;
                        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.OBTAIN_FOOD, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = 0, targetPOI = targetPOI }
                        , new Dictionary<INTERACTION_TYPE, object[]>() { { INTERACTION_TYPE.GET_FOOD, new object[] { neededFood } }, });
                        job.AllowDeadTargets();
                        characterThatWillDoJob.jobQueue.AddJobInQueue(job);
                        return true;
                    }
                }
            }
        }else if (targetPOI is TileObject) {
            TileObject objectToBeInspected = targetPOI as TileObject;
            if (objectToBeInspected.isSummonedByPlayer && !alreadyInspectedTileObjects.Contains(objectToBeInspected)) {
                if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, objectToBeInspected)) {
                    GoapPlanJob inspectJob = new GoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, objectToBeInspected);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(inspectJob);
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion
}
