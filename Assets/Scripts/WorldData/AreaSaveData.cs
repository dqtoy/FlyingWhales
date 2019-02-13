using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSaveData {
    public int areaID;
    public float recommendedPower;
    public string areaName;
    public AREA_TYPE areaType;
    public int coreTileID;
    public List<int> tileData; //list of tile id's that belong to this region
    public Color32 areaColor;
    public int ownerID;
    public int maxDefenderGroups;
    public int initialDefenderGroups;
    public int initialDefenderLevel;
    //public int supplyCapacity;
    public List<RACE> possibleOccupants;
    public List<InitialRaceSetup> raceSetup;
    //public int initialSupply;
    public int residentCapacity;
    public int monthlySupply;
    public int monthlyActions;
    //public List<string> possibleSpecialTokenSpawns;
    public int initialResidents;
    public Dictionary<STRUCTURE_TYPE, List<LocationStructure>> structures;
    public int dungeonSupplyRangeMin;
    public int dungeonSupplyRangeMax;

    public AreaSaveData(Area area) {
        areaID = area.id;
        areaName = area.name;
        areaType = area.areaType;
        coreTileID = area.coreTile.id;
        tileData = new List<int>();
        for (int i = 0; i < area.tiles.Count; i++) {
            HexTile currTile = area.tiles[i];
            tileData.Add(currTile.id);
        }
        areaColor = area.areaColor;
        if (area.owner == null) {
            ownerID = -1;
        } else {
            ownerID = area.owner.id;
        }
        maxDefenderGroups = area.maxDefenderGroups;
        initialDefenderGroups = area.initialDefenderGroups;
        //supplyCapacity = area.supplyCapacity;
        possibleOccupants = new List<RACE>(area.possibleOccupants);
        raceSetup = new List<InitialRaceSetup>(area.initialSpawnSetup);
        //initialSupply = area.initialSupply;
        //residentCapacity = area.residentCapacity;
        monthlySupply = area.monthlySupply;
        initialResidents = area.initialResidents;
        monthlyActions = area.monthlyActions;
        structures = area.structures;
        dungeonSupplyRangeMin = area.dungeonSupplyRangeMin;
        dungeonSupplyRangeMax = area.dungeonSupplyRangeMax;
        //possibleSpecialTokenSpawns = new List<string>();
        //for (int i = 0; i < area.possibleSpecialTokenSpawns.Count; i++) {
        //    SpecialToken currToken = area.possibleSpecialTokenSpawns[i];
        //    possibleSpecialTokenSpawns.Add(currToken.name);
        //}
    }
}
