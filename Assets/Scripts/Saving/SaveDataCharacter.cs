﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Traits;

[System.Serializable]
public class SaveDataCharacter {
    public int id;
    public string name;
    public string characterColorCode;
    //public int doNotDisturb;
    //public int doNotGetHungry;
    //public int doNotGetTired;
    //public int doNotGetLonely;
    public int factionID;
    public int homeID;
    public int homeStructureID;
    public int homeStructureAreaID;
    public STRUCTURE_TYPE homeStructureType;
    public int currentLocationID;
    public bool isDead;
    public GENDER gender;
    public SEXUALITY sexuality;
    public string className;
    public RACE race;
    public CHARACTER_ROLE roleType;
    public PortraitSettings portraitSettings;
    public ColorSave characterColor;
    public List<SaveDataTrait> normalTraits;

    //Stats
    public int currentHP;
    public int maxHP;
    public int level;
    public int experience;
    public int maxExperience;
    public int attackPowerMod;
    public int speedMod;
    public int maxHPMod;
    public int attackPowerPercentMod;
    public int speedPercentMod;
    public int maxHPPercentMod;

    public MORALITY morality;

    //public Dictionary<AlterEgoData, CharacterRelationshipData> relationships {
    //    get {
    //        return currentAlterEgo?.relationships ?? null;
    //    }
    //}
    //public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness {
    //    get {
    //        return currentAlterEgo.awareness;
    //    }
    //}

    //public List<INTERACTION_TYPE> currentInteractionTypes;
    public int supply;
    public List<SaveDataItem> items;
    public int moodValue;
    public bool isCombatant;
    public bool isDisabledByPlayer;
    public float speedModifier;
    public string deathStr;
    public int isStoppedByOtherCharacter;

    public POI_STATE state;

    //Needs
    public int tiredness;
    public int fullness;
    public int happiness;
    public int fullnessDecreaseRate;
    public int tirednessDecreaseRate;
    public int happinessDecreaseRate;

    //portrait
    public float hSkinColor;
    public float hHairColor;
    public float demonColor;

    //hostility
    public int ignoreHostility;

    //alter egos
    public string currentAlterEgoName;
    public List<SaveDataAlterEgo> alterEgos;

    public string originalClassName;
    public bool isMinion;
    public bool isSummon;
    public bool isFactionLeader;

    public int fullnessForcedTick;
    public int tirednessForcedTick;
    public TIME_IN_WORDS forcedFullnessRecoveryTimeInWords;
    public TIME_IN_WORDS forcedTirednessRecoveryTimeInWords;

    public bool returnedToLife;

    //For Summons Only
    public SUMMON_TYPE summonType;

    public Vector3Save gridTileLocation;
    public int gridTileLocationAreaID;

    public List<int> lethalHostilesInRangeIDs;
    public List<int> nonLethalHostilesInRangeIDs;
    public List<int> avoidInRangeIDs;

    public SaveDataCharacterState currentState;
    public bool hasCurrentState;

    public List<SaveDataJobQueueItem> jobs;

    public int currentSleepTicks;
    public int sleepScheduleJobID;
    public bool hasCancelledSleepSchedule;

    public List<SaveDataLog> history;

    public void Save(Character character) {
        id = character.id;
        name = character.name;
        characterColorCode = character.characterColorCode;
        //doNotDisturb = character.doNotDisturb;
        //doNotGetHungry = character.doNotGetHungry;
        //doNotGetLonely = character.doNotGetLonely;
        //doNotGetTired = character.doNotGetTired;
        if (character.faction != null) {
            factionID = character.faction.id;
            isFactionLeader = character.faction.leader == character;
        } else {
            factionID = -1;
        }
        if (character.homeRegion != null) {
            homeID = character.homeRegion.id;
        } else {
            homeID = -1;
        }
        if (character.specificLocation != null) {
            currentLocationID = character.specificLocation.id;
        } else {
            currentLocationID = -1;
        }
        if (character.homeStructure != null) {
            homeStructureID = character.homeStructure.id;
            homeStructureType = character.homeStructure.structureType;
            homeStructureAreaID = character.homeStructure.location.id;
        } else {
            homeStructureID = -1;
        }

        isDead = character.isDead;
        gender = character.gender;
        sexuality = character.sexuality;
        className = character.characterClass.className;
        race = character.race;
        roleType = character.role.roleType;
        portraitSettings = character.portraitSettings;
        characterColor = character.characterColor;
        isStoppedByOtherCharacter = character.isStoppedByOtherCharacter;

        normalTraits = new List<SaveDataTrait>();
        for (int i = 0; i < character.traitContainer.allTraits.Count; i++) {
            Trait trait = character.traitContainer.allTraits[i];

            SaveDataTrait saveDataTrait = SaveManager.ConvertTraitToSaveDataTrait(trait);
            if (saveDataTrait != null) {
                saveDataTrait.Save(trait);
                normalTraits.Add(saveDataTrait);
            }
        }

        currentHP = character.currentHP;
        maxHP = character.maxHP;
        level = character.level;
        experience = character.experience;
        maxExperience = character.maxExperience;
        attackPowerMod = character.attackPowerMod;
        speedMod = character.speedMod;
        maxHPMod = character.maxHPMod;
        attackPowerPercentMod = character.attackPowerPercentMod;
        speedPercentMod = character.speedPercentMod;
        maxHPPercentMod = character.maxHPPercentMod;

        //currentInteractionTypes = character.currentInteractionTypes;
        supply = character.supply;
        moodValue = character.moodValue;
        isCombatant = character.isCombatant;
        isDisabledByPlayer = character.isDisabledByPlayer;
        speedModifier = character.speedModifier;
        deathStr = character.deathStr;

        state = character.state;

        items = new List<SaveDataItem>();
        for (int i = 0; i < character.items.Count; i++) {
            SaveDataItem newSaveDataItem = new SaveDataItem();
            newSaveDataItem.Save(character.items[i]);
            items.Add(newSaveDataItem);
        }

        tiredness = character.tiredness;
        fullness = character.fullness;
        happiness = character.happiness;
        fullnessDecreaseRate = character.fullnessDecreaseRate;
        tirednessDecreaseRate = character.tirednessDecreaseRate;
        happinessDecreaseRate = character.happinessDecreaseRate;

        hSkinColor = character.hSkinColor;
        hHairColor = character.hHairColor;
        demonColor = character.demonColor;

        ignoreHostility = character.ignoreHostility;
        originalClassName = character.originalClassName;
        isMinion = character.minion != null;
        isSummon = character is Summon;
        if (isSummon) {
            Summon summon = character as Summon;
            summonType = summon.summonType;
        }

        currentAlterEgoName = character.currentAlterEgoName;
        alterEgos = new List<SaveDataAlterEgo>();
        foreach (AlterEgoData alterEgo in character.alterEgos.Values) {
            SaveDataAlterEgo saveDataAlterEgo = new SaveDataAlterEgo();
            saveDataAlterEgo.Save(alterEgo);
            alterEgos.Add(saveDataAlterEgo);
        }

        fullnessForcedTick = character.fullnessForcedTick;
        tirednessForcedTick = character.tirednessForcedTick;
        forcedFullnessRecoveryTimeInWords = character.forcedFullnessRecoveryTimeInWords;
        forcedTirednessRecoveryTimeInWords = character.forcedTirednessRecoveryTimeInWords;

        returnedToLife = character.returnedToLife;

        if (character.gridTileLocation != null) {
            gridTileLocation = new Vector3Save(character.gridTileLocation.localPlace.x, character.gridTileLocation.localPlace.y, 0f);
            gridTileLocationAreaID = character.gridTileLocation.structure.location.id;
        } else {
            gridTileLocation = new Vector3Save(0f, 0f, -1f);
        }

        if (character.marker != null) {
            lethalHostilesInRangeIDs = new List<int>();
            nonLethalHostilesInRangeIDs = new List<int>();
            avoidInRangeIDs = new List<int>();

            for (int i = 0; i < character.marker.hostilesInRange.Count; i++) {
                IPointOfInterest poi = character.marker.hostilesInRange[i];
                if (poi is Character) {
                    Character hostile = poi as Character;
                    if (character.marker.IsLethalCombatForTarget(hostile)) {
                        lethalHostilesInRangeIDs.Add(hostile.id);
                    } else {
                        nonLethalHostilesInRangeIDs.Add(hostile.id);
                    }
                }
            }
            for (int i = 0; i < character.marker.avoidInRange.Count; i++) {
                avoidInRangeIDs.Add(character.marker.avoidInRange[i].id);
            }
        }

        hasCurrentState = character.stateComponent.currentState != null && character.stateComponent.currentState.characterState != CHARACTER_STATE.COMBAT && character.stateComponent.currentState.job == null;
        if (hasCurrentState) {
            currentState = SaveUtilities.CreateCharacterStateSaveDataInstance(character.stateComponent.currentState);
            currentState.Save(character.stateComponent.currentState);
        }

        jobs = new List<SaveDataJobQueueItem>();
        for (int i = 0; i < character.jobQueue.jobsInQueue.Count; i++) {
            JobQueueItem job = character.jobQueue.jobsInQueue[i];
            if (job.isNotSavable) {
                continue;
            }
            //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
            SaveDataJobQueueItem data = null;
            if(job is GoapPlanJob) {
                data = new SaveDataGoapPlanJob();
            }else if(job is CharacterStateJob) {
                data = new SaveDataCharacterStateJob();
            }
            data.Save(job);
            jobs.Add(data);
        }

        currentSleepTicks = character.currentSleepTicks;
        sleepScheduleJobID = character.sleepScheduleJobID;
        hasCancelledSleepSchedule = character.hasCancelledSleepSchedule;

        history = new List<SaveDataLog>();
        for (int i = 0; i < character.history.Count; i++) {
            SaveDataLog data = new SaveDataLog();
            data.Save(character.history[i]);
            history.Add(data);
        }
    }

    public void Load() {
        Character character = null;
        if (isSummon) {
            character = CharacterManager.Instance.CreateNewSummon(this);
        } else {
            character = CharacterManager.Instance.CreateNewCharacter(this);
        }
        if (!isMinion && !isSummon && isDead) {
            //Do not process dead save data if character is a minion or summon, there is a separate process for that in Player
            character.ownParty.PartyDeath();
            if (character.role != null) {
                character.role.OnDeath(character);
            }
        }
    }

    public void LoadTraits(Character character) {
        for (int i = 0; i < normalTraits.Count; i++) {
            Character responsibleCharacter = null;
            Trait trait = normalTraits[i].Load(ref responsibleCharacter);
            character.traitContainer.AddTrait(character, trait, responsibleCharacter);
        }
        character.LoadAllStatsOfCharacter(this);
    }

    public void LoadRelationships(Character character) {
        for (int i = 0; i < alterEgos.Count; i++) {
            alterEgos[i].LoadRelationships(character);
        }
    }

    public void LoadHomeStructure(Character character) {
        if (homeStructureID != -1) {
            Area area = LandmarkManager.Instance.GetAreaByID(homeStructureAreaID);
            LocationStructure structure = area.GetStructureByID(homeStructureType, homeStructureID);
            character.MigrateHomeStructureTo(structure as Dwelling);
        }
    }

    public void LoadCharacterGridTileLocation(Character character) {
        if (gridTileLocation.z != -1f) {
            Area area = LandmarkManager.Instance.GetAreaByID(gridTileLocationAreaID);
            LocationGridTile gridTile = area.areaMap.map[(int) gridTileLocation.x, (int) gridTileLocation.y];

            if (character.marker == null) {
                character.CreateMarker();
            }
            character.LoadInitialCharacterPlacement(gridTile);

            for (int i = 0; i < lethalHostilesInRangeIDs.Count; i++) {
                Character target = CharacterManager.Instance.GetCharacterByID(lethalHostilesInRangeIDs[i]);
                character.marker.AddHostileInRange(target, isLethal: true);
            }
            for (int i = 0; i < nonLethalHostilesInRangeIDs.Count; i++) {
                Character target = CharacterManager.Instance.GetCharacterByID(nonLethalHostilesInRangeIDs[i]);
                character.marker.AddHostileInRange(target, isLethal: false);
            }
            for (int i = 0; i < avoidInRangeIDs.Count; i++) {
                Character target = CharacterManager.Instance.GetCharacterByID(avoidInRangeIDs[i]);
                character.marker.AddAvoidInRange(target);
            }

            if (character.isDead) {
                character.marker.OnDeath(gridTile);
            }
        }
    }

    /// <summary>
    /// Load the characters current state. NOTE: This should only load states that are not combat and are not part of jobs.
    /// </summary>
    /// <param name="character"></param>
    public void LoadCharacterCurrentState(Character character) {
        if (hasCurrentState) {
            Character targetCharacter = null;
            Area targetArea = null;
            if (currentState.targetCharacterID != -1) {
                targetCharacter = CharacterManager.Instance.GetCharacterByID(currentState.targetCharacterID);
            }
            //if (currentState.targetAreaID != -1) {
            //    targetArea = LandmarkManager.Instance.GetAreaByID(currentState.targetAreaID);
            //}
            CharacterState loadedState = character.stateComponent.SwitchToState(currentState.characterState, targetCharacter, targetArea, currentState.duration, currentState.level);
            loadedState.SetCurrentDuration(currentState.currentDuration);
            loadedState.SetIsUnending(currentState.isUnending);

            if (currentState.isPaused) {
                loadedState.PauseState();
            }
        }
    }

    public void LoadCharacterJobs(Character character) {
        for (int i = 0; i < jobs.Count; i++) {
            JobQueueItem job = jobs[i].Load();
            character.jobQueue.AddJobInQueue(job);

            //if (jobs[i] is SaveDataCharacterStateJob) {
            //    SaveDataCharacterStateJob dataStateJob = jobs[i] as SaveDataCharacterStateJob;
            //    CharacterStateJob stateJob = job as CharacterStateJob;
            //    if (dataStateJob.assignedCharacterID != -1) {
            //        Character assignedCharacter = CharacterManager.Instance.GetCharacterByID(dataStateJob.assignedCharacterID);
            //        stateJob.SetAssignedCharacter(assignedCharacter);
            //        CharacterState newState = assignedCharacter.stateComponent.SwitchToState(stateJob.targetState, null, stateJob.targetArea);
            //        if (newState != null) {
            //            stateJob.SetAssignedState(newState);
            //        } else {
            //            throw new System.Exception(assignedCharacter.name + " tried doing state " + stateJob.targetState.ToString() + " but was unable to do so! This must not happen!");
            //        }
            //    }
            //}
        }
    }

    public void LoadCharacterHistory(Character character) {
        for (int i = 0; i < history.Count; i++) {
            character.AddHistory(history[i].Load());
        }
    }
}
