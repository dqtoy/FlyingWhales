using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class SaveDataArea {
    public int id;
    public string name;
    public bool isDead;
    public AREA_TYPE areaType;
    public int citizenCount;
    public int coreTileID;
    public ColorSave areaColor;
    public int ownerID;
    //public int previousOwnerID;
    public List<int> tileIDs;
    //public List<int> residentIDs;
    //public List<int> charactersAtLocationIDs;
    public List<SaveDataItem> itemsInArea;

    //public List<Log> history;
    //public JobQueue jobQueue;
    //public LocationStructure prison;
    //public List<RACE> possibleOccupants;
    //public List<InitialRaceSetup> initialSpawnSetup;

    public void Save(Area area) {
        id = area.id;
        name = area.name;
        isDead = area.isDead;
        areaType = area.areaType;
        citizenCount = area.citizenCount;
        coreTileID = area.coreTile.id;
        areaColor = area.areaColor;
        ownerID = area.owner.id;
        //if(area.previousOwner != null) {
        //    previousOwnerID = area.previousOwner.id;
        //} else {
        //    previousOwnerID = -1;
        //}

        tileIDs = new List<int>();
        for (int i = 0; i < area.tiles.Count; i++) {
            tileIDs.Add(area.tiles[i].id);
        }

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
    }

    public void Load() {
        Area newArea = LandmarkManager.Instance.CreateNewArea(this);
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
}
