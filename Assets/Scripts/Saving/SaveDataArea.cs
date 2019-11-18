using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataArea {
    public int id;
    //public bool isDead;
    public AREA_TYPE areaType;
    public int citizenCount;
    public int regionID;
    //public int coreTileID;
    //public ColorSave areaColor;
    //public int ownerID;
    //public int previousOwnerID;
    //public List<int> tileIDs;
    //public List<int> residentIDs;
    //public List<int> charactersAtLocationIDs;
    public List<SaveDataItem> itemsInArea;

    //public List<Log> history;
    //public JobQueue jobQueue;
    //public LocationStructure prison;
    //public List<RACE> possibleOccupants;
    //public List<InitialRaceSetup> initialSpawnSetup;
    public List<SaveDataLocationStructure> structures;
    public List<SaveDataJobQueueItem> jobs;

    public void Save(Area area) {
        id = area.id;
        //isDead = area.isDead;
        areaType = area.areaType;
        citizenCount = area.citizenCount;
        regionID = area.region.id;
        //coreTileID = area.coreTile.id;
        //areaColor = area.areaColor;
        //ownerID = area.owner.id;
        //if(area.previousOwner != null) {
        //    previousOwnerID = area.previousOwner.id;
        //} else {
        //    previousOwnerID = -1;
        //}

        //tileIDs = new List<int>();
        //for (int i = 0; i < area.tiles.Count; i++) {
        //    tileIDs.Add(area.tiles[i].id);
        //}

        //residentIDs = new List<int>();
        //for (int i = 0; i < area.areaResidents.Count; i++) {
        //    residentIDs.Add(area.areaResidents[i].id);
        //}

        //charactersAtLocationIDs = new List<int>();
        //for (int i = 0; i < area.charactersAtLocation.Count; i++) {
        //    charactersAtLocationIDs.Add(area.charactersAtLocation[i].id);
        //}

        itemsInArea = new List<SaveDataItem>();
        for (int i = 0; i < area.itemsInArea.Count; i++) {
            SaveDataItem newSaveDataItem = new SaveDataItem();
            newSaveDataItem.Save(area.itemsInArea[i]);
            itemsInArea.Add(newSaveDataItem);
        }

        structures = new List<SaveDataLocationStructure>();
        foreach (KeyValuePair<STRUCTURE_TYPE, List<LocationStructure>> kvp in area.structures) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                SaveDataLocationStructure data = new SaveDataLocationStructure();
                data.Save(kvp.Value[i]);
                structures.Add(data);
            }
        }

        jobs = new List<SaveDataJobQueueItem>();
        for (int i = 0; i < area.availableJobs.Count; i++) {
            JobQueueItem job = area.availableJobs[i];
            if (job.isNotSavable) {
                continue;
            }
            //SaveDataJobQueueItem data = System.Activator.CreateInstance(System.Type.GetType("SaveData" + job.GetType().ToString())) as SaveDataJobQueueItem;
            SaveDataJobQueueItem data = null;
            if (job is GoapPlanJob) {
                data = new SaveDataGoapPlanJob();
            } else if (job is CharacterStateJob) {
                data = new SaveDataCharacterStateJob();
            }
            data.Save(job);
            jobs.Add(data);
        }
    }

    public void Load() {
        Area newArea = LandmarkManager.Instance.CreateNewArea(this);
        newArea.region.SetArea(newArea); //Set area of region here not on SaveDataRegion because SaveDataRegion will be the first to load, there will be no areas there yet
        if(newArea.areaType != AREA_TYPE.DEMONIC_INTRUSION) {
            LandmarkManager.Instance.SetEnemyPlayerArea(newArea);
        }
    }
    //Loading area items is called separately because of sequencing issues
    //Since loading an item requires faction owner, if this is called in Load(), there is still no faction owner yet, so it will be an issue
    //The sequence for loading save data is LoadAreas -> LoadFactions -> LoadAreaItems, so as to ensure that the area already has a faction owner when loading the items and by that logic the items loaded will also have a faction owner
    public void LoadAreaItems() {
        Area area = LandmarkManager.Instance.GetAreaByID(id);
        for (int i = 0; i < itemsInArea.Count; i++) {
            itemsInArea[i].Load(area);
        }
    }

    public void LoadStructureEntranceTiles() {
        for (int i = 0; i < structures.Count; i++) {
            structures[i].LoadEntranceTile();
        }
    }

    public void LoadAreaJobs() {
        Area area = LandmarkManager.Instance.GetAreaByID(id);
        for (int i = 0; i < jobs.Count; i++) {
            JobQueueItem job = jobs[i].Load();
            area.AddToAvailableJobs(job);
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
}
