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
    public List<int> residentIDs;
    public List<int> charactersAtLocationIDs;

    //public List<Log> history;
    //public JobQueue jobQueue;
    //public LocationStructure prison;
    //public List<RACE> possibleOccupants;
    //public List<InitialRaceSetup> initialSpawnSetup;
    //public List<SpecialToken> possibleSpecialTokenSpawns;

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

        residentIDs = new List<int>();
        for (int i = 0; i < area.areaResidents.Count; i++) {
            residentIDs.Add(area.areaResidents[i].id);
        }

        charactersAtLocationIDs = new List<int>();
        for (int i = 0; i < area.charactersAtLocation.Count; i++) {
            charactersAtLocationIDs.Add(area.charactersAtLocation[i].id);
        }
    }

    public void Load() {
        Area newArea = LandmarkManager.Instance.CreateNewArea(this);
    }
}
