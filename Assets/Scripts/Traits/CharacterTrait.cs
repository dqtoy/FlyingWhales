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
        }
        if (targetPOI is TileObject) {
            TileObject tileObj = targetPOI as TileObject;
            if (tileObj.isSummonedByPlayer && characterThatWillDoJob.GetNormalTrait("Suspicious") == null && !alreadyInspectedTileObjects.Contains(tileObj)) {
                if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.INSPECT, tileObj)) {
                    GoapPlanJob inspectJob = new GoapPlanJob(JOB_TYPE.INSPECT, INTERACTION_TYPE.INSPECT, tileObj);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(inspectJob);
                    return true;
                }
            } else if (tileObj is GoddessStatue) {
                if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.PRAY_GODDESS_STATUE, tileObj) && tileObj.state == POI_STATE.ACTIVE) {
                    GoapPlanJob prayJob = new GoapPlanJob(JOB_TYPE.PRAY_GODDESS_STATUE, INTERACTION_TYPE.PRAY_TILE_OBJECT, tileObj);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(prayJob);
                    return true;
                }
            }
        }
        if (targetPOI is SpecialToken) {
            if(characterThatWillDoJob.role.roleType != CHARACTER_ROLE.BEAST) {
                SpecialToken token = targetPOI as SpecialToken;
                if (token.characterOwner == null) {
                    //Patrollers should not pick up items from their warehouse
                    if (token.structureLocation != null && token.structureLocation.structureType == STRUCTURE_TYPE.WAREHOUSE
                        && token.specificLocation == characterThatWillDoJob.homeArea) {
                        return false;
                    }
                    if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_ITEM_GOAP)) {
                        GoapPlanJob pickUpJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PICK_ITEM_GOAP, token);
                        characterThatWillDoJob.jobQueue.AddJobInQueue(pickUpJob);
                    }
                    return true;
                }
            }
        }
        if (targetPOI is Character || targetPOI is Tombstone) {
            Character deadTarget = null;
            if(targetPOI is Character) {
                deadTarget = targetPOI as Character;
            }else if(targetPOI is Tombstone) {
                deadTarget = (targetPOI as Tombstone).character;
            }
            if (deadTarget.isDead) {
                Dead deadTrait = deadTarget.GetNormalTrait("Dead") as Dead;
                if (!deadTrait.charactersThatSawThisDead.Contains(characterThatWillDoJob)) {
                    deadTrait.AddCharacterThatSawThisDead(characterThatWillDoJob);

                    Log sawDeadLog = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "saw_dead");
                    sawDeadLog.AddToFillers(characterThatWillDoJob, characterThatWillDoJob.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    sawDeadLog.AddToFillers(deadTarget, deadTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    characterThatWillDoJob.AddHistory(sawDeadLog);
                    PlayerManager.Instance.player.ShowNotificationFrom(sawDeadLog, characterThatWillDoJob, false);


                    if (characterThatWillDoJob.HasRelationshipOfTypeWith(deadTarget, RELATIONSHIP_TRAIT.LOVER)) {
                        characterThatWillDoJob.AddTrait("Heartbroken");
                        bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                        characterThatWillDoJob.AdjustHappiness(-6000);
                        return hasCreatedJob;
                    } else if (characterThatWillDoJob.HasRelationshipOfTypeWith(deadTarget, RELATIONSHIP_TRAIT.RELATIVE)) {
                        characterThatWillDoJob.AddTrait("Griefstricken");
                        bool hasCreatedJob = RandomizeBetweenShockAndCryJob(characterThatWillDoJob);
                        characterThatWillDoJob.AdjustHappiness(-4000);
                        return hasCreatedJob;
                    } else if (characterThatWillDoJob.HasRelationshipOfTypeWith(deadTarget, RELATIONSHIP_TRAIT.FRIEND)) {
                        characterThatWillDoJob.AddTrait("Griefstricken");
                        bool hasCreatedJob = CreatePrioritizedShockJob(characterThatWillDoJob);
                        characterThatWillDoJob.AdjustHappiness(-2000);
                        return hasCreatedJob;
                    }
                }
            }
        }
        return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
    }
    #endregion

    private bool RandomizeBetweenShockAndCryJob(Character characterThatWillDoJob) {
        if(UnityEngine.Random.Range(0, 2) == 0) {
            return CreatePrioritizedShockJob(characterThatWillDoJob);
        } else {
            return CreatePrioritizedCryJob(characterThatWillDoJob);
        }
    }
    private bool CreatePrioritizedShockJob(Character characterThatWillDoJob) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.PRIORITIZED_SHOCK)) {
            GoapPlanJob shockJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PRIORITIZED_SHOCK, characterThatWillDoJob);
            characterThatWillDoJob.jobQueue.AddJobInQueue(shockJob);
            return true;
        }
        return false;
    }
    private bool CreatePrioritizedCryJob(Character characterThatWillDoJob) {
        if (!characterThatWillDoJob.jobQueue.HasJob(JOB_TYPE.MISC, INTERACTION_TYPE.PRIORITIZED_CRY)) {
            GoapPlanJob cryJob = new GoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.PRIORITIZED_CRY, characterThatWillDoJob);
            characterThatWillDoJob.jobQueue.AddJobInQueue(cryJob);
            return true;
        }
        return false;
    }
}

public class SaveDataCharacterTrait : SaveDataTrait {
    public List<TileObjectSerializableData> alreadyInspectedTileObjects;

    public override void Save(Trait trait) {
        base.Save(trait);
        alreadyInspectedTileObjects = new List<TileObjectSerializableData>();
        CharacterTrait derivedTrait = trait as CharacterTrait;
        for (int i = 0; i < derivedTrait.alreadyInspectedTileObjects.Count; i++) {
            TileObject to = derivedTrait.alreadyInspectedTileObjects[i];
            TileObjectSerializableData toData = new TileObjectSerializableData {
                id = to.id,
                type = to.tileObjectType,
            };
            alreadyInspectedTileObjects.Add(toData);
        }
    }

    public override Trait Load(ref Character responsibleCharacter) {
        Trait trait = base.Load(ref responsibleCharacter);
        CharacterTrait derivedTrait = trait as CharacterTrait;
        for (int i = 0; i < alreadyInspectedTileObjects.Count; i++) {
            TileObjectSerializableData toData = alreadyInspectedTileObjects[i];
            derivedTrait.AddAlreadyInspectedObject(InteriorMapManager.Instance.GetTileObject(toData.type, toData.id));
        }
        return trait;
    }
}
