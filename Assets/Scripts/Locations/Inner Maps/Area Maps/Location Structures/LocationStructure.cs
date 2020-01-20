using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BayatGames.SaveGameFree.Types;
using Inner_Maps;
using UnityEngine.Assertions;

[System.Serializable]
public class LocationStructure {
    public int id { get; private set; }
    public string name { get; private set; }
    public STRUCTURE_TYPE structureType { get; private set; }
    public List<Character> charactersHere { get; private set; }
    public ILocation location { get; private set; }
    public Settlement settlementLocation => tiles[0].buildSpotOwner.hexTileOwner.settlementOnTile;
    public List<SpecialToken> itemsInStructure { get; private set; }
    public List<IPointOfInterest> pointsOfInterest { get; private set; }
    public POI_STATE state { get; private set; }
    public LocationStructureObject structureObj {get; private set;}
    public BuildSpotTileObject occupiedBuildSpot { get; private set; }

    //Inner Map
    public List<LocationGridTile> tiles { get; private set; }
    public List<LocationGridTile> unoccupiedTiles { get; private set; }
    public LocationGridTile entranceTile { get; private set; }

    #region getters
    public virtual bool isDwelling => false;
    #endregion

    public LocationStructure(STRUCTURE_TYPE structureType, ILocation location) {
        id = Utilities.SetID(this);
        this.structureType = structureType;
        name = $"{Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString())} {id.ToString()}";
        this.location = location;
        charactersHere = new List<Character>();
        itemsInStructure = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        tiles = new List<LocationGridTile>();
        unoccupiedTiles = new List<LocationGridTile>();
        SubscribeListeners();
    }
    public LocationStructure(ILocation location, SaveDataLocationStructure data) {
        this.location = location;
        id = Utilities.SetID(this, data.id);
        structureType = data.structureType;
        name = data.name;
        charactersHere = new List<Character>();
        itemsInStructure = new List<SpecialToken>();
        pointsOfInterest = new List<IPointOfInterest>();
        tiles = new List<LocationGridTile>();
        SubscribeListeners();
    }

    #region Listeners
    private void SubscribeListeners() {
        if (structureType.IsOpenSpace() == false) {
            Messenger.AddListener<WallObject>(Signals.WALL_DAMAGED, OnWallDamaged);
            Messenger.AddListener<WallObject>(Signals.WALL_DESTROYED, OnWallDestroyed);
            Messenger.AddListener<WallObject>(Signals.WALL_REPAIRED, OnWallRepaired);
        }
    }
    private void UnsubscribeListeners() {
        if (structureType.IsOpenSpace() == false) {
            Messenger.RemoveListener<WallObject>(Signals.WALL_DAMAGED, OnWallDamaged);
            Messenger.RemoveListener<WallObject>(Signals.WALL_DESTROYED, OnWallDestroyed);
            Messenger.RemoveListener<WallObject>(Signals.WALL_REPAIRED, OnWallRepaired);
        }
    }
    #endregion

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
        if (!itemsInStructure.Contains(token)) {
            itemsInStructure.Add(token);
            token.SetStructureLocation(this);
            if(AddPOI(token, gridLocation)) {
                if (token.gridTileLocation.buildSpotOwner.hexTileOwner != null 
                    && token.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile != null) {
                    token.SetOwner(token.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile.owner);
                    token.gridTileLocation.buildSpotOwner.hexTileOwner.settlementOnTile?.OnItemAddedToLocation(token, this);
                }
            }
        }
    }
    public void RemoveItem(SpecialToken token, Character removedBy = null) {
        if (itemsInStructure.Remove(token)) {
            token.SetStructureLocation(null);
            LocationGridTile removedFrom = token.gridTileLocation;
            if (RemovePOI(token, removedBy)) {
                removedFrom.buildSpotOwner.hexTileOwner.settlementOnTile?.OnItemRemovedFromLocation(token, this);
            }
        }
    }
    public void OwnItemsInLocation(Faction owner) {
        for (int i = 0; i < itemsInStructure.Count; i++) {
            itemsInStructure[i].SetOwner(owner);
        }
    }
    public int GetItemsOfTypeCount(SPECIAL_TOKEN type) {
        int count = 0;
        for (int i = 0; i < itemsInStructure.Count; i++) {
            if (itemsInStructure[i].specialTokenType == type) {
                count++;
            }
        }
        return count;
    }
    #endregion

    #region Points Of Interest
    public virtual bool AddPOI(IPointOfInterest poi, LocationGridTile tileLocation = null, bool placeObject = true) {
        if (!pointsOfInterest.Contains(poi)) {
            if (placeObject) {
                if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                    if (!PlaceAreaObjectAtAppropriateTile(poi, tileLocation)) { return false; }
                }
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
                    location.innerMap.RemoveObject(poi.gridTileLocation, removedBy);
                }
                //throw new System.Exception("Provided tile of " + poi.ToString() + " is null!");
            }
            return true;
        }
        return false;
    }
    public virtual bool RemovePOIWithoutDestroying(IPointOfInterest poi) {
        if (pointsOfInterest.Remove(poi)) {
            if (poi.gridTileLocation != null) {
                if (poi.poiType != POINT_OF_INTEREST_TYPE.CHARACTER) {
                    location.innerMap.RemoveObjectWithoutDestroying(poi.gridTileLocation);
                }
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
    public List<T> GetTileObjectsOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject {
        List<T> objs = new List<T>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i] is TileObject) {
                TileObject obj = pointsOfInterest[i] as TileObject;
                if (obj.tileObjectType == type) {
                    objs.Add(obj as T);
                }
            }
        }
        return objs;
    }
    public List<T> GetTileObjectsOfType<T>() where T : TileObject {
        List<T> objs = new List<T>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i] is T) {
                T obj = pointsOfInterest[i] as T;
                objs.Add(obj);
            }
        }
        return objs;
    }
    public T GetTileObjectOfType<T>(TILE_OBJECT_TYPE type) where T : TileObject{
        List<TileObject> objs = new List<TileObject>();
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i] is TileObject) {
                TileObject obj = pointsOfInterest[i] as TileObject;
                if (obj.tileObjectType == type) {
                    return obj as T;
                }
            }
        }
        return null;
    }
    public ResourcePile GetResourcePileObjectWithLowestCount(TILE_OBJECT_TYPE type) {
        ResourcePile chosenPile = null;
        int lowestCount = 0;
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i] is ResourcePile) {
                ResourcePile obj = pointsOfInterest[i] as ResourcePile;
                if (obj.tileObjectType == type) {
                    if(chosenPile == null || obj.resourceInPile < lowestCount) {
                        chosenPile = obj;
                        lowestCount = obj.resourceInPile;
                    }
                }
            }
        }
        return chosenPile;
    }
    private bool PlaceAreaObjectAtAppropriateTile(IPointOfInterest poi, LocationGridTile tile) {
        if (tile != null) {
            location.innerMap.PlaceObject(poi, tile);
            return true;
        } else {
            List<LocationGridTile> tilesToUse;
            if (location.locationType == LOCATION_TYPE.DEMONIC_INTRUSION) { //player settlement
                tilesToUse = tiles;
            } else {
                tilesToUse = GetValidTilesToPlace(poi);
            }
            if (tilesToUse.Count > 0) {
                LocationGridTile chosenTile = tilesToUse[Random.Range(0, tilesToUse.Count)];
                location.innerMap.PlaceObject(poi, chosenTile);
                return true;
            } 
            // else {
            //     Debug.LogWarning("There are no tiles at " + structureType.ToString() + " at " + location.name + " for " + poi.ToString());
            // }
        }
        return false;
    }
    private List<LocationGridTile> GetValidTilesToPlace(IPointOfInterest poi) {
        switch (poi.poiType) {
            case POINT_OF_INTEREST_TYPE.TILE_OBJECT:
                if (poi is MagicCircle) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && !x.HasNeighbourOfType(LocationGridTile.Tile_Type.Wall)).ToList();
                } else if (poi is WaterWell) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && x.parentMap.GetTilesInRadius(x, 3).Where(y => y.objHere is WaterWell).Count() == 0 && !x.HasNeighbouringWalledStructure()).ToList();
                } else if (poi is GoddessStatue) {
                    return unoccupiedTiles.Where(x => !x.HasOccupiedNeighbour() && x.parentMap.GetTilesInRadius(x, 3).Where(y => y.objHere is GoddessStatue).Count() == 0 && !x.HasNeighbouringWalledStructure()).ToList();
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
                return "the " + Utilities.NormalizeStringUpperCaseFirstLetters(structureType.ToString());
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
    public TileObject GetUnoccupiedTileObject(params TILE_OBJECT_TYPE[] type) {
        for (int i = 0; i < pointsOfInterest.Count; i++) {
            if (pointsOfInterest[i].IsAvailable() && pointsOfInterest[i] is TileObject) {
                TileObject tileObj = pointsOfInterest[i] as TileObject;
                if (type.Contains(tileObj.tileObjectType) && tileObj.mapObjectState == MAP_OBJECT_STATE.BUILT) {
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
    public void SetOccupiedBuildSpot(BuildSpotTileObject buildSpotTileObject) {
        occupiedBuildSpot = buildSpotTileObject;
    }
    #endregion

    #region Destroy
    private void DestroyStructure() {
        Debug.Log($"{GameManager.Instance.TodayLogString()}{ToString()} was destroyed!");
        //transfer tiles to either the wilderness or work settlement
        List<LocationGridTile> tilesInStructure = new List<LocationGridTile>(tiles);
        LocationStructure workArea = location.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
        LocationStructure wilderness = location.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        for (int i = 0; i < tilesInStructure.Count; i++) {
            LocationGridTile tile = tilesInStructure[i];
            LocationStructure transferTo;
            if (tile.isInside) {
                transferTo = workArea;
            } else {
                transferTo = wilderness;
            }

            tile.ClearWallObjects();

            tile.SetStructure(transferTo);
            if (tile.objHere != null) {
                if (tile.objHere is SpecialToken) {
                    AddItem(tile.objHere as SpecialToken, tile);
                } else {
                    AddPOI(tile.objHere, tile);
                }
            }
            tile.SetPreviousGroundVisual(null); //so that the tile will never revert to the structure tile, unless a new structure is put on it.
            tile.genericTileObject.AdjustHP(tile.genericTileObject.maxHP);
        }
        if (settlementLocation != null) {
            Settlement settlement = settlementLocation;
            JobQueueItem existingRepairJob = settlement.GetJob(JOB_TYPE.REPAIR, occupiedBuildSpot);
            if (existingRepairJob != null) {
                settlement.RemoveFromAvailableJobs(existingRepairJob);
            }    
        }
        
        occupiedBuildSpot.RemoveOccupyingStructure(this);
        ObjectPoolManager.Instance.DestroyObject(structureObj.gameObject);
        location.RemoveStructure(this);
        settlementLocation.RemoveStructure(this);
        Messenger.Broadcast(Signals.STRUCTURE_OBJECT_REMOVED, this, occupiedBuildSpot);
        SetOccupiedBuildSpot(null);
    }
    private bool CheckIfStructureDestroyed() {
        string summary = $"Checking if {ToString()} has been destroyed...";
        //check walls and floors, if all of them are destroyed consider this structure as destroyed
        bool allObjectsDestroyed = true;
        for (int i = 0; i < structureObj.walls.Length; i++) {
            WallObject wall = structureObj.walls[i];
            if (wall.currentHP > 0) {
                //wall is not yet destroyed
                summary += $"\n{ToString()} still has an intact wall. Not yet destroyed.";
                allObjectsDestroyed = false;
                break;
            }
        }

        if (allObjectsDestroyed) {
            //check floor tiles
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                if (tile.genericTileObject.currentHP > 0) {
                    summary += $"\n{ToString()} still has an intact floor. Not yet destroyed.";
                    allObjectsDestroyed = false;
                    break;
                }
            }
        }

        //if at end of checking, all objects are destroyed, then consider this structure as destroyed
        if (allObjectsDestroyed) {
            summary += $"\n{ToString()} has no intact walls or floors. It has been destroyed.";
            DestroyStructure();
        }
        Debug.Log(summary);
        return allObjectsDestroyed;
    }
    #endregion

    #region Walls
    public void OnWallDestroyed(WallObject wall) {
        //check if structure destroyed
        if (structureObj.walls.Contains(wall)) {
            wall.gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Empty);
            structureObj.RescanPathfindingGridOfStructure();
            CheckIfStructureDestroyed();
        }
    }
    public void OnWallRepaired(WallObject wall) {
        if (structureObj.walls.Contains(wall)) {
            wall.gridTileLocation.SetTileType(LocationGridTile.Tile_Type.Wall);
        }
    }
    public void OnWallDamaged(WallObject wall) {
        Assert.IsNotNull(structureObj, $"Wall of {this.ToString()} was damaged, but it has no structure object");
        if (structureObj.walls.Contains(wall)) {
            //create repair job
            OnStructureDamaged();
        }
    }
    public void OnTileDamaged(LocationGridTile tile) {
        OnStructureDamaged();
    }
    public void OnTileRepaired(LocationGridTile tile) {
        structureObj?.ApplyGroundTileAssetForTile(tile);
    }
    public void OnTileDestroyed(LocationGridTile tile) {
        if (structureType.IsOpenSpace()) {
            return; //do not check for destruction if structure is open space (Wilderness, Work Settlement, Cemetery, etc.)
        }
        CheckIfStructureDestroyed();
    }
    private void OnStructureDamaged() {
        if (structureType.IsOpenSpace() || structureType.IsSettlementStructure() == false) {
            return; //do not check for damage if structure is open space (Wilderness, Work Settlement, Cemetery, etc.)
        }
        if (occupiedBuildSpot.advertisedActions.Contains(INTERACTION_TYPE.REPAIR_STRUCTURE) == false) {
            occupiedBuildSpot.AddAdvertisedAction(INTERACTION_TYPE.REPAIR_STRUCTURE);
        }
        if (settlementLocation != null) {
            if (settlementLocation.HasJob(JOB_TYPE.REPAIR, occupiedBuildSpot) == false) {
                CreateRepairJob();
            }    
        }
        
    }
    private bool StillHasObjectsToRepair() {
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.genericTileObject.currentHP < tile.genericTileObject.maxHP) {
                return true;
            }
            for (int j = 0; j < tile.walls.Count; j++) {
                WallObject wall = tile.walls[j];
                if (wall.currentHP < wall.maxHP) {
                    return true;
                }
            }
        }
        return false;
    }
    #endregion

    #region Repair
    private void CreateRepairJob() {
        if (settlementLocation != null) {
            GoapPlanJob repairJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.REPAIR, INTERACTION_TYPE.REPAIR_STRUCTURE, occupiedBuildSpot, settlementLocation);
            repairJob.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeRepairStructureJob);
            settlementLocation.AddToAvailableJobs(repairJob);    
        }
    }
    #endregion

    #region Resource
    public void ChangeResourceMadeOf(RESOURCE resource) {
        structureObj.ChangeResourceMadeOf(resource);
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
        state = structure.state;

        if(structure.entranceTile != null) {
            entranceTile = new Vector3Save(structure.entranceTile.localPlace.x, structure.entranceTile.localPlace.y, 0);
        } else {
            entranceTile = new Vector3Save(0f,0f,-1f);
        }
    }

    public LocationStructure Load(ILocation location) {
        LocationStructure createdStructure = null;
        switch (structureType) {
            case STRUCTURE_TYPE.DWELLING:
                createdStructure = new Dwelling(location, this);
                break;
            default:
                createdStructure = new LocationStructure(location, this);
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