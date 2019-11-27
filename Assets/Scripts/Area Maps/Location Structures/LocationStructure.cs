﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree.Types;

[System.Serializable]
public class LocationStructure {
    public int id { get; private set; }
    public string name { get; private set; }
    public STRUCTURE_TYPE structureType { get; private set; }
    public bool isInside { get; private set; }
    public List<Character> charactersHere { get; private set; }
    [System.NonSerialized]
    private Area _location;
    private List<SpecialToken> _itemsHere;
    public List<IPointOfInterest> pointsOfInterest { get; private set; }
    public POI_STATE state { get; private set; }
    public LocationStructureObject structureObj {get; private set;}

    //Inner Map
    public List<LocationGridTile> tiles { get; private set; }
    public List<LocationGridTile> unoccupiedTiles { get; private set; }
    public LocationGridTile entranceTile { get; private set; }

    #region getters
    public Area location {
        get { return _location; }
    }
    public List<SpecialToken> itemsInStructure {
        get { return _itemsHere; }
    }
    #endregion

    public LocationStructure(STRUCTURE_TYPE structureType, Area location, bool isInside) {
        id = Utilities.SetID(this);
        this.structureType = structureType;
        this.name = Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
        this.isInside = isInside;
        _location = location;
        charactersHere = new List<Character>();
        _itemsHere = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        tiles = new List<LocationGridTile>();
        unoccupiedTiles = new List<LocationGridTile>();
    }
    public LocationStructure(Area location, SaveDataLocationStructure data) {
        _location = location;
        id = Utilities.SetID(this, data.id);
        this.structureType = data.structureType;
        this.name = data.name;
        this.isInside = data.isInside;
        charactersHere = new List<Character>();
        _itemsHere = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        tiles = new List<LocationGridTile>();
    }

    #region Residents
    public virtual bool IsOccupied() {
        return false; //will only ever use this in dwellings, to prevent need for casting
    }
    #endregion

    #region Characters
    public void AddCharacterAtLocation(Character character, LocationGridTile tile = null) {
        if (!charactersHere.Contains(character)) {
            charactersHere.Add(character);
            //location.AddCharacterToLocation(character);
            AddPOI(character, tile);
        } else {
            //Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be added to " + ToString() + " because it is already there!");
        }
        character.SetCurrentStructureLocation(this);
    }
    public void RemoveCharacterAtLocation(Character character) {
        if (charactersHere.Remove(character)) {
            character.SetCurrentStructureLocation(null);
            RemovePOI(character);
        } else {
            //Debug.LogWarning(GameManager.Instance.TodayLogString() + " " + character.name + " can't be removed from " + ToString() + " because it is not there!");
        }
    }
    #endregion

    #region Items/Special Tokens
    public void AddItem(SpecialToken token, LocationGridTile gridLocation = null) {
        if (!_itemsHere.Contains(token)) {
            _itemsHere.Add(token);
            token.SetStructureLocation(this);
            AddPOI(token, gridLocation);
        }
    }
    public void RemoveItem(SpecialToken token) {
        if (_itemsHere.Remove(token)) {
            token.SetStructureLocation(null);
            RemovePOI(token);
        }
    }
    public void OwnItemsInLocation(Faction owner) {
        for (int i = 0; i < _itemsHere.Count; i++) {
            _itemsHere[i].SetOwner(owner);
        }
    }
    public int GetItemsOfTypeCount(SPECIAL_TOKEN type) {
        int count = 0;
        for (int i = 0; i < _itemsHere.Count; i++) {
            if (_itemsHere[i].specialTokenType == type) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Points Of Interest
    public virtual bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null) {
        if (!pointsOfInterest.Contains(poi)) {
            if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                if (!PlaceAreaObjectAtAppropriateTile(poi, tileLocation)) { return false; }
            }
            pointsOfInterest.Add(poi);
            return true;
        }
        return false;
    }
    public virtual bool RemovePOI(IPointOfInterest poi, Character removedBy = null) {
        if (pointsOfInterest.Remove(poi)) {
            if (poi.gridTileLocation != null) {
                //Debug.Log("Removed " + poi.ToString() + " from " + poi.gridTileLocation.ToString() + " at " + this.ToString());
                if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
                    //location.areaMap.RemoveCharacter(poi.gridTileLocation, poi as Character);
                } else {
                    location.areaMap.RemoveObject(poi.gridTileLocation, removedBy);
                }
                //throw new System.Exception("Provided tile of " + poi.ToString() + " is null!");
            }
            return true;
        }
        return false;
    }
    public List<IPointOfInterest> GetPOIsOfType(POINT_OF_INTEREST_TYPE type) {
        List<IPointOfInterest> pois = new List<IPointOfInterest>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i].poiType == type) {
                pois.Add(pointsOfInterest[i]);
            }
        }
        return pois;
    }
    public List<TileObject> GetTileObjectsOfType(TILE_OBJECT_TYPE type) {
        List<TileObject> objs = new List<TileObject>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i] is TileObject) {
                TileObject obj = pointsOfInterest[i] as TileObject;
                if (obj.tileObjectType == type) {
                    objs.Add(obj);
                }
            }
        }
        return objs;
    }
    private bool PlaceAreaObjectAtAppropriateTile(IPointOfInterest poi, LocationGridTile tile) {
        if (tile != null) {
            location.areaMap.PlaceObject(poi, tile);
            return true;
        } else {
            List<LocationGridTile> tilesToUse;
            if (location.areaType == AREA_TYPE.DEMONIC_INTRUSION) { //player area
                tilesToUse = tiles;
            } else {
                tilesToUse = GetValidTilesToPlace(poi);
            }
            if (tilesToUse.Count > 0) {
                LocationGridTile chosenTile = tilesToUse[Random.Range(0, tilesToUse.Count)];
                location.areaMap.PlaceObject(poi, chosenTile);
                return true;
            } else {
                Debug.LogWarning("There are no tiles at " + structureType.ToString() + " at " + location.name + " for " + poi.ToString());
            }
        }
        return false;
    }
    private List<LocationGridTile> GetValidTilesToPlace(IPointOfInterest poi) {
        switch (poi.poiType) {
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                if (poi is MagicCircle) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)).ToList();
                } else if (poi is WaterWell) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && x.parentAreaMap.GetTilesInRadius(x, 3).Where(y => y.objHere is WaterWell).Count() == 0 && !x.HasNeighbouringWalledStructure()).ToList();
                } else if (poi is GoddessStatue) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && x.parentAreaMap.GetTilesInRadius(x, 3).Where(y => y.objHere is GoddessStatue).Count() == 0 && !x.HasNeighbouringWalledStructure()).ToList();
                } else if (poi is Guitar || poi is Bed || poi is Table) {
                    return GetOuterTiles().Where(x => unoccupiedTiles.Contains(x) && x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList();
                } else {
                    return unoccupiedTiles.Where(x => x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList(); ;
                }
            case POINT_OF_INTEREST_TYPE.CHARACTER:
                return unoccupiedTiles;
            default:
                return unoccupiedTiles.Where(x => !x.IsAdjacentTo(typeof(MagicCircle)) && x.tileType != LocationGridTile.Tile_Type.Structure_Entrance).ToList();
        }
    }
    #endregion   
    
    #region Tiles
    public void AddTile(LocationGridTile tile) {
        if (!tiles.Contains(tile)) {
            tiles.Add(tile);
            if(tile.tileState == LocationGridTile.Tile_State.Empty) {
                AddUnoccupiedTile(tile);
            } else {
                RemoveUnoccupiedTile(tile);
            }
        }
    }
    public void RemoveTile(LocationGridTile tile) {
        tiles.Remove(tile);
        RemoveUnoccupiedTile(tile);
    }
    public void AddUnoccupiedTile(LocationGridTile tile) {
        unoccupiedTiles.Add(tile);
    }
    public void RemoveUnoccupiedTile(LocationGridTile tile) {
        unoccupiedTiles.Remove(tile);
    }
    public LocationGridTile GetRandomTile() {
        if (tiles.Count <= 0) {
            return null;
        }
        return tiles[Random.Range(0, tiles.Count)];
    }
    public void SetEntranceTile(LocationGridTile tile) {
        entranceTile = tile;
    }
    #endregion

    #region Utilities
    /// <summary>
    /// Get the structure's name based on specified rules.
    /// Rules are at - https://trello.com/c/mRzzH9BE/1432-location-naming-convention
    /// </summary>
    /// <param name="character">The character requesting the name</param>
    public virtual string GetNameRelativeTo(Character character) {
        switch (structureType) {
            case STRUCTURE_TYPE.INN:
                return "the inn";
            case STRUCTURE_TYPE.WAREHOUSE:
                return "the " + location.name + " warehouse";
            case STRUCTURE_TYPE.PRISON:
                return "the " + location.name + " prison";
            case STRUCTURE_TYPE.WILDERNESS:
                return "the outskirts of " + location.name;
            case STRUCTURE_TYPE.CEMETERY:
                return "the cemetery of " + location.name;
            case STRUCTURE_TYPE.DUNGEON:
            case STRUCTURE_TYPE.WORK_AREA:
            case STRUCTURE_TYPE.EXPLORE_AREA:
            case STRUCTURE_TYPE.POND:
                return location.name;
            case STRUCTURE_TYPE.CITY_CENTER:
                return "the " + location.name + " city center";
            default:
                return ToString();
        }
    }
    public List<LocationGridTile> GetOuterTiles() {
        List<LocationGridTile> outerTiles = new List<LocationGridTile>();
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.HasDifferentDwellingOrOutsideNeighbour()) {
                outerTiles.Add(currTile);
            }
        }
        return outerTiles;
    }
    public void DoCleanup() {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            IPointOfInterest poi = pointsOfInterest[i];
            if (poi is TileObject) {
                (poi as TileObject).DoCleanup();
            } else if (poi is SpecialToken) {
                (poi as SpecialToken).DoCleanup();
            }
        }
    }
    #endregion

    #region Tile Objects
    protected List<TileObject> GetTileObjects() {
        List<TileObject> objs = new List<TileObject>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            IPointOfInterest currPOI = pointsOfInterest[i];
            if (currPOI is TileObject) {
                objs.Add(currPOI as TileObject);
            }
        }
        return objs;
    }
    public List<TileObject> GetTileObjectsThatAdvertise(params INTERACTION_TYPE[] types) {
        List<TileObject> objs = new List<TileObject>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            IPointOfInterest currPOI = pointsOfInterest[i];
            if (currPOI is TileObject) {
                TileObject obj = currPOI as TileObject;
                if (obj.IsAvailable() && obj.AdvertisesAll(types)) {
                    objs.Add(obj);
                }
            }
        }
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile currTile = tiles[i];
            if (currTile.genericTileObject.IsAvailable() && currTile.genericTileObject.AdvertisesAll(types)) {
                objs.Add(currTile.genericTileObject);
            }
        }
        return objs;
    }
    public TileObject GetUnoccupiedTileObject(TILE_OBJECT_TYPE type) {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i].IsAvailable() && pointsOfInterest[i] is TileObject) {
                TileObject tileObj = pointsOfInterest[i] as TileObject;
                if (tileObj.tileObjectType == type) {
                    return tileObj;
                }
            }
        }
        return null;
    }
    public TileObject GetUnoccupiedTileObject(TILE_OBJECT_TYPE type1, TILE_OBJECT_TYPE type2) {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i].IsAvailable() && pointsOfInterest[i] is TileObject) {
                TileObject tileObj = pointsOfInterest[i] as TileObject;
                if (tileObj.tileObjectType == type1 || tileObj.tileObjectType == type2) {
                    return tileObj;
                }
            }
        }
        return null;
    }
    #endregion

    #region Structure Objects
    public void SetStructureObject(LocationStructureObject structureObj) {
        this.structureObj = structureObj;
    }
    #endregion

    public override string ToString() {
        return structureType.ToString() + " " + id.ToString() + " at " + location.name;
    }
}

[System.Serializable]
public class SaveDataLocationStructure {
    public int id;
    public string name;
    public STRUCTURE_TYPE structureType;
    public bool isInside;
    public POI_STATE state;

    public Vector3Save entranceTile;
    public bool isFromTemplate;

    private LocationStructure loadedStructure;
    public void Save(LocationStructure structure) {
        id = structure.id;
        name = structure.name;
        structureType = structure.structureType;
        isInside = structure.isInside;
        state = structure.state;

        if(structure.entranceTile != null) {
            entranceTile = new Vector3Save(structure.entranceTile.localPlace.x, structure.entranceTile.localPlace.y, 0);
        } else {
            entranceTile = new Vector3Save(0f,0f,-1f);
        }
    }

    public LocationStructure Load(Area area) {
        LocationStructure createdStructure = null;
        switch (structureType) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(area, this);
                break;
            default:
                createdStructure = new LocationStructure(area, this);
                break;
        }
        loadedStructure = createdStructure;
        return createdStructure;
    }

    //This is loaded last so release loadedStructure
    public void LoadEntranceTile() {
        if(entranceTile.z != -1f) {
            for (int i = 0; i < loadedStructure.tiles.Count; i++) {
                LocationGridTile tile = loadedStructure.tiles[i];
                if(tile.localPlace.x == (int)entranceTile.x && tile.localPlace.y == (int) entranceTile.y) {
                    loadedStructure.SetEntranceTile(tile);
                    break;
                }
            }
        }
        loadedStructure = null;
    }
}